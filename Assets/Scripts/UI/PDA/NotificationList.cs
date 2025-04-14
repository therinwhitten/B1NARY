namespace B1NARY
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;

	/// <summary>
	/// A list that tracks notifications if they have played and inspected through
	/// or not. Functions as first come permanent serve.
	/// </summary>
	public class NotificationList
	{
		public event Action<UnlockableFlag> NewNotification;

		private List<UnlockableFlag> allNotifications = new();
		private HashSet<string> flags = new();

		public NotificationList(IReadOnlyList<string> serializedNotifications = null)
		{
			if (serializedNotifications == null)
				return;
			for (int i = 0; i < serializedNotifications.Count; i++)
				AddNewNotificationToList(UnlockableFlag.FromString(serializedNotifications[i]));
		}

		/// <summary>
		/// Sets all the finished/unfinished, etc. notifications to start playing
		/// again to the specified <paramref name="panelTo"/>.
		/// </summary>
		/// <param name="panelTo"></param>
		public void Restart(NotificationPanel panelTo)
		{
			for (int i = 0; i < allNotifications.Count; i++)
				if (allNotifications[i].IsActive)
				{
					panelTo.ForceAddNotification(allNotifications[i]);
				}
		}

		/// <summary>
		/// Adds a brand-new notification to the list.
		/// </summary>
		/// <param name="flag"></param>
		/// <returns>will return <see langword="true"/> if it has a unique flag,
		/// and must be active. Otherwise, <see langword="false"/>.</returns>
		public bool AddNewNotificationToList(UnlockableFlag flag)
		{
			if (!flag.IsActive)
				return false;
			if (flags.Contains(flag.FlagName))
				return false;
			allNotifications.Add(flag);
			flags.Add(flag.FlagName);
			NewNotification?.Invoke(flag);
			return true;
		}

		public string[] GetSerializedNotifications()
		{
			string[] strings = new string[allNotifications.Count];
			for (int i = 0; i < allNotifications.Count; i++)
				strings[i] = allNotifications[i].ToString();
			return strings;
		}
	}

}
