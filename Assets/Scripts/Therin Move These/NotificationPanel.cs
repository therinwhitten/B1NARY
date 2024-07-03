namespace B1NARY
{
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
		internal List<NotificationBehaviour> Notifications = new();
		[SerializeField]
		public GameObject NotificationPrefab;

		[SerializeField]
		public UnityEvent<int> NotificationRecieved = new();


		public void Awake()
		{
			CollectibleCollection.UnlockedUnlockableEvent += PlayNewNotification;
			IReadOnlyList<string> notifs = PlayerConfig.Instance.uncheckedNotifications;
			for (int i = 0; i < notifs.Count; i++)
				PlayNewNotification(new CollectibleCollection.NewFlag("", "", notifs[i]));
		}
		public void OnDestroy()
		{
			CollectibleCollection.UnlockedUnlockableEvent -= PlayNewNotification;
		}


		public void PlayNewNotification(CollectibleCollection.NewFlag flag)
		{
			// Create new notification
			GameObject obj = Instantiate(NotificationPrefab, transform, false);
			obj.SetActive(false);
			var behaviour = obj.GetComponentInChildren<NotificationBehaviour>();
			string formalName = flag.FormalName;
			behaviour.SetText(flag);
			behaviour.closeButton.onClick.AddListener(() => PlayerConfig.Instance.uncheckedNotifications.Remove(formalName));
			obj.SetActive(true);
			PlayerConfig.Instance.uncheckedNotifications.Add(formalName);
			NotificationRecieved.Invoke(Notifications.Count - 1);
		}
	}
}
