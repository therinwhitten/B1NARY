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
		public IReadOnlyList<NotificationBehaviour> Notifications => notifications;
		internal List<NotificationBehaviour> notifications = new();
		[SerializeField]
		public GameObject NotificationPrefab;

		[SerializeField]
		public UnityEvent<int> NotificationRecieved = new();
		[SerializeField]
		public UnityEvent<bool> SetActived = new();
		[SerializeField]
		public UnityEvent NoNewNotifications = new();

		[SerializeField]
		public CustomAudioClip NotificationNotification;

		private readonly Dictionary<string, NotificationBehaviour> existingNotifications = new();

		private HashSet<string> ignoreNotifications = new();

		private PersistentFlag this[int index]
		{
			get => PersistentFlag.FromString(PlayerConfig.Instance.uncheckedNotifications[index]);
			set => PlayerConfig.Instance.uncheckedNotifications[index] = value.ToString();
		}

		public void Awake()
		{
			CollectibleCollection.UnlockedUnlockableEvent += PlayNewNotification;
			IReadOnlyList<string> notifs = PlayerConfig.Instance.uncheckedNotifications;
			for (int i = 0; i < notifs.Count; i++)
			{
				PersistentFlag newFlag = PersistentFlag.FromString(notifs[i]);
				PlayNewNotification(newFlag);
				if (!newFlag.VeryCool)
					ignoreNotifications.Add(newFlag.Flag.FlagName);
			}
			NotificationBehaviour[] collection = gameObject.GetComponentsInChildren<NotificationBehaviour>();
			existingNotifications.EnsureCapacity(collection.Length);
			for (int i = 0; i < collection.Length; i++)
				if (!string.IsNullOrWhiteSpace(collection[i].flagKey))
					existingNotifications[collection[i].flagKey] = collection[i];
		}
		public void OnDestroy()
		{
			CollectibleCollection.UnlockedUnlockableEvent -= PlayNewNotification;
		}
		public void OnEnable()
		{
			SetActived.Invoke(true);
		}
		public void OnDisable()
		{
			SetActived.Invoke(false);
		}

		public void PlayNewNotification(PersistentFlag flag)
		{
			if (!flag.VeryCool)
				return;
			PlayNewNotification(flag.Flag);
		}
		public void PlayNewNotification(NewFlag flag)
		{
			if (ignoreNotifications.Contains(flag.FlagName))
				return;
			string formalName = flag.FormalName;
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
			behaviour.closeButton.onClick.AddListener(RemovedNotification);
			NotificationRecieved.Invoke(notifications.Count - 1);
			AudioController.Instance.AddSound(NotificationNotification);

			void RemovedNotification()
			{
				ignoreNotifications.Add(flag.FlagName);
				this[index] = new(false, flag);
				if (PlayerConfig.Instance.uncheckedNotifications.Count == ignoreNotifications.Count)
					NoNewNotifications.Invoke();
			}
		}

		public bool TryGetExistingNotification(NewFlag flag, out NotificationBehaviour existingObject) 
			=> existingNotifications.TryGetValue(flag.FlagName, out existingObject);

		public record PersistentFlag(bool VeryCool, NewFlag Flag)
		{
			public override string ToString() => $"{VeryCool}/{Flag.Type}/{Flag.FlagName}/{Flag.FormalName}";
			public static PersistentFlag FromString(string value)
			{
				string[] split = value.Split('/');
				return new PersistentFlag(bool.Parse(split[0]), new NewFlag(split[1], split[2], split[3]));
			}
		}
	}
}
