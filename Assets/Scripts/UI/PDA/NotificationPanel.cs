namespace B1NARY
{
	using B1NARY.Audio;
	using B1NARY.DataPersistence;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using TMPro;
	using UnityEngine;
	using UnityEngine.Events;
	using UnityEngine.UI;
	using static B1NARY.DataPersistence.CollectibleCollection;

	public class NotificationPanel : MonoBehaviour
	{
		public IReadOnlyList<NotificationBehaviour> ActiveNotifications => activeNotifications;
		internal List<NotificationBehaviour> activeNotifications = new();
		[SerializeField]
		public GameObject NotificationPrefab;

		[SerializeField]
		public UnityEvent<int> NotificationRecieved = new();
		[SerializeField]
		public UnityEvent NoNewNotifications = new();

		/// <summary>
		/// Sound that plays when a new notif appears.
		/// </summary>
		[SerializeField]
		public CustomAudioClip NotificationNotification;

		/// <summary>
		/// All the notifications that are disabled inside the notifications panel.
		/// </summary>
		private readonly Dictionary<string, NotificationBehaviour> existingNotifications = new();
		/// <summary>
		/// Blocks certain notifications from playing again.
		/// </summary>
		private readonly HashSet<string> ignoreNotifications = new();

		private PersistentFlag this[int index]
		{
			get => PersistentFlag.FromString(PlayerConfig.Instance.uncheckedNotifications[index]);
			set => PlayerConfig.Instance.uncheckedNotifications[index] = value.ToString();
		}

		public void Awake()
		{
			CollectibleCollection.UnlockedUnlockableEvent += PlayNewNotification;
			NotificationBehaviour[] collection = gameObject.GetComponentsInChildren<NotificationBehaviour>(true);
			existingNotifications.EnsureCapacity(collection.Length);
			for (int i = 0; i < collection.Length; i++)
				if (!string.IsNullOrWhiteSpace(collection[i].flagKey))
					existingNotifications[collection[i].flagKey] = collection[i];
			IReadOnlyList<string> notifs = PlayerConfig.Instance.uncheckedNotifications;
			for (int i = 0; i < notifs.Count; i++)
			{
				PersistentFlag newFlag = PersistentFlag.FromString(notifs[i]);
				PlayNewNotification(newFlag);
				if (!newFlag.NotPlayed)
					ignoreNotifications.Add(newFlag.Flag.FlagName);
			}
		}
		public void OnDestroy()
		{
			CollectibleCollection.UnlockedUnlockableEvent -= PlayNewNotification;
		}

		public void PlayNewNotification(PersistentFlag flag)
		{
			if (!flag.NotPlayed)
				return;
			PlayNewNotification(flag.Flag);
		}
		public void PlayNewNotification(NewFlag flag)
		{
			if (ignoreNotifications.Contains(flag.FlagName))
				return;
			string formalName = flag.FlagName;
			int index = PlayerConfig.Instance.uncheckedNotifications.IndexOf(flag.FlagName);
			if (index == -1)
			{ 
				PlayerConfig.Instance.uncheckedNotifications.Add(null); 
				index = PlayerConfig.Instance.uncheckedNotifications.Count - 1; 
			}
			this[index] = new(true, flag);
			if (TryGetExistingNotification(flag, out NotificationBehaviour behaviour))
			{
				behaviour.gameObject.SetActive(true);
			}
			else // Try fallback option, creates new notif from prefab
			{
				Debug.LogWarning($"Missing notification for flag '{flag.FlagName}'! languages may be inaccurate!");
				GameObject obj = Instantiate(NotificationPrefab, transform, false);
				obj.SetActive(false);
				behaviour = obj.GetComponentInChildren<NotificationBehaviour>();
				behaviour.SetText(flag);
				obj.SetActive(true);
			}
			string parsedValue = flag.ToString();
			activeNotifications.Add(behaviour);
			behaviour.closeButton.onClick.AddListener(RemovedNotification);
			NotificationRecieved.Invoke(activeNotifications.Count - 1);
			AudioController.Instance.AddSound(NotificationNotification);

			void RemovedNotification()
			{
				ignoreNotifications.Add(flag.FlagName);
				this[index] = new(false, flag);
				activeNotifications.Remove(behaviour);
				if (activeNotifications.Count <= 0)
					NoNewNotifications.Invoke();
			}
		}

		public bool TryGetExistingNotification(NewFlag flag, out NotificationBehaviour existingObject) 
			=> existingNotifications.TryGetValue(flag.FlagName, out existingObject);

		public record PersistentFlag(bool NotPlayed, NewFlag Flag)
		{
			public override string ToString() => $"{NotPlayed}/{Flag.Type}/{Flag.FlagName}"; 
			public static PersistentFlag FromString(string value)
			{
				string[] split = value.Split('/');
				if (!bool.TryParse(split[0], out bool resultBool))
					throw new FormatException($"Failed to serialize bool from '{value}'!");
				return new PersistentFlag(bool.Parse(split[0]), new NewFlag(split[1], split[2]));
			}
		}
	}
}
