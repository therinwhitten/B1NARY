namespace B1NARY
{
	using System.Collections.Generic;
	using System.Linq;
	using System;
	using UnityEngine;

	/// <summary>
	/// A class that tracks both persistent and non-persistent listeners. Roughly
	/// similar to <see cref="UnityEngine.Events.UnityEvent"/>.
	/// </summary>
	[Serializable]
	public class ListenerGroup
	{
		public static ListenerGroup operator +(ListenerGroup group, Action action)
		{
			group.AddPersistentListener(action);
			return group;
		}

		public ListenerGroup(IEnumerable<Action> persistentListeners, IEnumerable<Action> nonPersistentListeners)
		{
			this.persistentListeners = new List<Action>(persistentListeners);
			this.nonPersistentListeners = new List<Action>(nonPersistentListeners);
		}
		public ListenerGroup(IEnumerable<Action> persistentListeners)
		{
			this.persistentListeners = new List<Action>(persistentListeners);
			nonPersistentListeners = new List<Action>();
		}
		public ListenerGroup()
		{
			persistentListeners = new List<Action>();
			nonPersistentListeners = new List<Action>();
		}

		/// <summary>
		/// Invokes all <see cref="persistentListeners"/> and 
		/// <see cref="nonPersistentListeners"/>. Clears all 
		/// <see cref="nonPersistentListeners"/> afterwards.
		/// </summary>
		public virtual void InvokeAll()
		{
			for (int i = 0; i < persistentListeners.Count; i++)
				persistentListeners[i].Invoke();
			for (int i = 0; i < nonPersistentListeners.Count; i++)
				nonPersistentListeners[i].Invoke();
			nonPersistentListeners.Clear();
		}
		protected readonly List<Action> persistentListeners;
		protected readonly List<Action> nonPersistentListeners;
		/// <summary>
		/// All persistent listeners that stay after being added so it would be
		/// invoked multiple times.
		/// </summary>
		public IReadOnlyList<Action> PersistentListeners => persistentListeners;
		/// <summary>
		/// All non-persistent listeners that dont stay after being invoked, so
		/// it is invoked once after being added.
		/// </summary>
		public IReadOnlyList<Action> NonPersistentListeners => nonPersistentListeners;


		/// <summary>
		/// Adds a single persistent listener that listens to an event multiple
		/// times and persists.
		/// </summary>
		/// <param name="delegate"> The delegate to invoke multiple times. </param>
		/// <exception cref="InvalidOperationException"/>
		public void AddPersistentListener(Action @delegate)
		{
			if (persistentListeners.Contains(@delegate))
				throw new InvalidOperationException();
			persistentListeners.Add(@delegate);
		}
		/// <summary>
		/// Removes a single persistent listener that lsitens to an event multiple
		/// times.
		/// </summary>
		/// <param name="delegate"> The delegate that is invoked multiple times. </param>
		public void RemovePersistentListener(Action @delegate) =>
			persistentListeners.Remove(@delegate);
		/// <summary>
		/// Adds a non-persistent listener that gets destroyed after being invoked
		/// once.
		/// </summary>
		/// <param name="delegate"> The delegate that is invoked once. </param>
		/// <exception cref="InvalidOperationException"/>
		public void AddNonPersistentListener(Action @delegate)
		{
			if (nonPersistentListeners.Contains(@delegate))
				throw new InvalidOperationException();
			nonPersistentListeners.Add(@delegate);
		}
		/// <summary>
		/// Removes a non-Persistent Listener.
		/// </summary>
		/// <param name="delegate">An void delegate with no arguments. </param>
		public void RemoveNonPersistentListener(Action @delegate)
			=> nonPersistentListeners.Remove(@delegate);

		public IEnumerable<Action> AsEnumerable()
		{
			for (int i = 0; i < persistentListeners.Count; i++)
				yield return persistentListeners[i];
			for (int i = 0; i < nonPersistentListeners.Count; i++)
				yield return nonPersistentListeners[i];
		}
	}
}