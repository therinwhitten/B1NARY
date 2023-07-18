namespace B1NARY
{
	using System;
	using System.Collections.Generic;
	using UnityEngine.SceneManagement;
	using Object = UnityEngine.Object;
	using SceneManagerSex = UnityEngine.SceneManagement.SceneManager;

	public sealed class Pauser : IDisposable
	{
		public Pauser()
		{
			SceneManagerSex.activeSceneChanged += UpdateOnScene;
		}
		public void Dispose()
		{
			SceneManagerSex.activeSceneChanged -= UpdateOnScene;
		}
		private void UpdateOnScene(Scene old, Scene @new) => _ = Blocking;
		private readonly LinkedList<Object> Sources = new LinkedList<Object>();


		public bool Blocking
		{
			get
			{
				// Updating sources
				LinkedListNode<Object> current = Sources.First;
				while (current != null)
				{
					LinkedListNode<Object> next = current.Next;
					if (current.Value == null)
						Sources.Remove(current);
					current = next;
				}

				// Getting return value
				bool output = broken || Sources.Count > 0;
				if (output != m_blocking)
				{
					m_blocking = output;
					BlockingChanged?.Invoke(output);
				}
				return output;
			}
		}
		private bool m_blocking = false;
		private bool broken = false;
		public event Action<bool> BlockingChanged;

		public bool RemoveBlocker(Object source)
		{
			bool output = Sources.Remove(source);
			_ = Blocking;
			return output;
		}
		public void AddBlocker(Object source)
		{
			if (Sources.Contains(source))
				return;
			Sources.AddLast(source);
			_ = Blocking;
		}
		public void ToggleBlocker(Object source)
		{
			if (!Sources.Remove(source))
				Sources.AddLast(source);
			_ = Blocking;
		}
		public void ToggleBlocker(Object source, bool toggle)
		{
			if (toggle)
				AddBlocker(source);
			else
				RemoveBlocker(source);
		}
		public void Break()
		{
			broken = true;
			_ = Blocking;
		}
	}
}