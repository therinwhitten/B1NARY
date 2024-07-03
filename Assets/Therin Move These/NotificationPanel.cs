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

	public record Notification(GameObject Object, string formalName, Action<string> SetText)
	{
		public void Dispose()
		{
			Object.SetActive(false);
			PlayerConfig.Instance.uncheckedNotifications.Remove(formalName);
		}
	}
	public class NotificationPanel : MonoBehaviour
	{
		/*
		public HashSet<string> Flags
		{
			get
			{
				if (_flags is not null)
					return _flags;
				PlayerConfig.Instance.collectibles.MergeSavesFromSingleton();
				IEnumerable<string> merger = PlayerConfig.Instance.collectibles;
				_flags = new HashSet<string>(merger);
				return _flags;
			}
		}
		private HashSet<string> _flags;
		*/


		internal List<Notification> Notifications = new();
		[SerializeField]
		public GameObject NotificationPrefab = new();

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
			var text = obj.GetComponentInChildren<TMP_Text>();
			Notification notif = new(obj, flag.FormalName, set => text.text = set);
			Notifications.Add(notif);
			// on button press, close it
			obj.GetComponentInChildren<Button>().onClick.AddListener(notif.Dispose);

			text.text = flag.FormalName;
			obj.SetActive(true);
			PlayerConfig.Instance.uncheckedNotifications.Add(flag.FormalName);
			NotificationRecieved.Invoke(Notifications.Count - 1);
		}
	}
}
