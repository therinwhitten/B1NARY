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

		public void Awake()
		{
			CollectibleCollection.UnlockedUnlockableEvent += PlayNewNotification;
			IReadOnlyList<string> notifs = PlayerConfig.Instance.uncheckedNotifications;
			for (int i = 0; i < notifs.Count; i++)
				PlayNewNotification(CollectibleCollection.NewFlag.FromString(notifs[i]));
			NotificationBehaviour[] collection = gameObject.GetComponentsInChildren<NotificationBehaviour>();
			existingNotifications.EnsureCapacity(collection.Length);
			for (int i = 0; i < collection.Length; i++)
				existingNotifications.Add(collection[i].flagKey, collection[i]);
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


		public void PlayNewNotification(CollectibleCollection.NewFlag flag)
		{
			string formalName = flag.FormalName;
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
			if (!PlayerConfig.Instance.uncheckedNotifications.Contains(parsedValue))
				PlayerConfig.Instance.uncheckedNotifications.Add(parsedValue);
			NotificationRecieved.Invoke(notifications.Count - 1);
			AudioController.Instance.AddSound(NotificationNotification);

			void RemovedNotification()
			{
				PlayerConfig.Instance.uncheckedNotifications.Remove(parsedValue);
				if (PlayerConfig.Instance.uncheckedNotifications.Count <= 0)
					NoNewNotifications.Invoke();
			}
		}

		public bool TryGetExistingNotification(CollectibleCollection.NewFlag flag, out NotificationBehaviour existingObject) 
			=> existingNotifications.TryGetValue(flag.FlagName, out existingObject);
	}
}
