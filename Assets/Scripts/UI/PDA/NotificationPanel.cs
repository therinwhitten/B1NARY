namespace B1NARY
{
	using B1NARY.Audio;
	using B1NARY.DataPersistence;
	using B1NARY.DesignPatterns;
	using HDConsole;
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

	public class NotificationPanel : DesignPatterns.Singleton<NotificationPanel>
	{
		#region Events
		/// <summary>
		/// Unlocks unlockables, usually by <see cref="SaveSlot"/> first, but if it
		/// is not available, then <see cref="PlayerConfig"/> instead.
		/// </summary>
		/// <param name="type">the library it will add to.</param>
		/// <param name="flagName">the actual flag name.</param>
		/// <exception cref="InvalidOperationException"></exception>
		[Command("bny_unlock_unlockable")]
		public static void UnlockUnlockable(string type, string flagName)
		{
			UnlockableFlag flag = new() { FlagName = flagName, IsActive = true, Category = type };
			type = type.ToLower();
			if (SaveSlot.ActiveSlot == null)
			{
				// Missing savefile, saving directly to config instead.
				HashSet<string> saveTo = type switch
				{
					UNLOCKED_GALLERY_KEY => PlayerConfig.Instance.collectibles.Gallery,
					UNLOCKED_MAP_KEY => PlayerConfig.Instance.collectibles.Map,
					UNLOCKED_CHAR_KEY => PlayerConfig.Instance.collectibles.CharacterProfiles,
					_ => throw new InvalidOperationException($"type '{type}' is not valid!")
				};
				saveTo.Add(flagName);
				return;
			}
			// Saving onto existing savefile
			List<string> target = type switch
			{
				UNLOCKED_GALLERY_KEY => SaveSlot.ActiveSlot.collectibles.Gallery,
				UNLOCKED_MAP_KEY => SaveSlot.ActiveSlot.collectibles.Map,
				UNLOCKED_CHAR_KEY => SaveSlot.ActiveSlot.collectibles.CharacterProfiles,
				_ => throw new InvalidOperationException($"type '{type}' is not valid!")
			};
			if (target.Contains(flagName))
				return;
			target.Add(flagName);
			UnlockedUnlockableEvent?.Invoke(flag);
		}
		public static event Action<UnlockableFlag> UnlockedUnlockableEvent;
		#endregion
		/// <summary>
		/// Sound that plays when a new notif appears.
		/// </summary>
		[SerializeField]
		public CustomAudioClip NotificationNotification;

		public IReadOnlyList<NotificationBehaviour> ActiveNotifications => activeNotifications;
		/// <summary>
		/// Notifications that are shown to the player.
		/// </summary>
		internal List<NotificationBehaviour> activeNotifications = new();

		[SerializeField]
		public GameObject NotificationPrefab;

		[SerializeField]
		public UnityEvent NoNewNotifications = new();


		/// <summary>
		/// All the notifications that are disabled inside the notifications panel.
		/// </summary>
		private readonly Dictionary<string, NotificationBehaviour> premadeNotifications = new();
		public bool TryGetExistingNotification(UnlockableFlag flag, out NotificationBehaviour existingObject)
			=> premadeNotifications.TryGetValue(flag.FlagName, out existingObject);

		/// <summary>
		/// A list that tracks all notifications that has been played or playing,
		/// not specifically active.
		/// </summary>
		public NotificationList PairedList { get; private set; }

		protected override void SingletonAwake()
		{
			PairedList = new NotificationList();
			PairedList.Restart(this);
			UnlockedUnlockableEvent += _notifEvent;

			// Find existing/preparsed notifications
			NotificationBehaviour[] collection = gameObject.GetComponentsInChildren<NotificationBehaviour>(true);
			premadeNotifications.EnsureCapacity(collection.Length);
			for (int i = 0; i < collection.Length; i++)
				if (!string.IsNullOrWhiteSpace(collection[i].flagKey))
					premadeNotifications[collection[i].flagKey] = collection[i];
		}
		protected override void OnSingletonDestroy()
		{
			UnlockedUnlockableEvent -= _notifEvent;
		}

		private void _notifEvent(UnlockableFlag flag) => AddNewNotification(flag);
		
		/// <summary>
		/// This checks (and adds to) the <see cref="PairedList"/> before invoking
		/// <see cref="ForceAddNotification(UnlockableFlag)"/>.
		/// </summary>
		/// <param name="flag"></param>
		/// <returns></returns>
		internal bool AddNewNotification(UnlockableFlag flag)
		{
			bool pass = PairedList.AddNewNotificationToList(flag);
			if (!pass)
				return false;
			ForceAddNotification(flag);
			return true;
		}

		/// <summary>
		/// Forcefully adds a new active notification that is seen by the player,
		/// doesn't interact with <see cref="PairedList"/>.
		/// </summary>
		/// <param name="flag"></param>
		internal void ForceAddNotification(UnlockableFlag flag)
		{
			flag.IsActive = true; // Just in case

			// Delivering Notification
			if (premadeNotifications.TryGetValue(flag.FlagName, out NotificationBehaviour behaviour))
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

			// Attaching behaviour to panel
			string parsedValue = flag.ToString();
			activeNotifications.Add(behaviour);
			behaviour.pairedFlag = flag;
			behaviour.closeButton.onClick.AddListener(RemovedNotification);
			AudioController.Instance.AddSound(NotificationNotification);
			void RemovedNotification()
			{
				activeNotifications.Remove(behaviour);
				behaviour.pairedFlag.IsActive = false;

				if (activeNotifications.Count <= 0)
					NoNewNotifications.Invoke();
			}
		}
	}
}
