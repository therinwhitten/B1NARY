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
	public sealed class ListenerGroup
	{
		public static ListenerGroup operator +(ListenerGroup group, Action action)
		{
			group.AddPersistentListener(action);
			return group;
		}

		/// <summary>
		/// A single delegate that invokes all methods in the array. No clearing
		/// needed, as long as <see cref="InvokeAll"/> is called.
		/// </summary>
		public Action<Action[]> loadingAPIDelegate = actions =>
		{
			for (int i = 0; i < actions.Length; i++)
			{
				try
				{
					actions[i]?.Invoke();
				}
				catch (Exception ex)
				{
					Debug.LogException(ex);
				}
			}
		};

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
		public void InvokeAll()
		{
			Action[] delegates =
				new List<Action>[] { persistentListeners, nonPersistentListeners }
				.SelectMany(list => list).ToArray();
			nonPersistentListeners.Clear();
			loadingAPIDelegate.Invoke(delegates);
		}
		private readonly List<Action> persistentListeners;
		private readonly List<Action> nonPersistentListeners;
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