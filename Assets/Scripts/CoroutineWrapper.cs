namespace B1NARY
{
	using System;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Collections;
	using UnityEngine;

	public sealed class CoroutineWrapper : IDisposable
	{
		public static bool IsNotRunningOrNull(CoroutineWrapper coroutineWrapper)
		{
			if (coroutineWrapper == null)
				return true;
			return !coroutineWrapper.IsRunning;
		}

		[Obsolete("Use the chainable command .Start()")]
		public static CoroutineWrapper StartNew(MonoBehaviour tiedMonoBehaviour, IEnumerator enumerator)
		{
			CoroutineWrapper wrapper = new(tiedMonoBehaviour, enumerator);
			wrapper.Start();
			return wrapper;
		}
		public Task Awaiter => Task.Run(() => SpinWait.SpinUntil(() => !IsRunning));
		private Coroutine coroutineData = null;
		private readonly MonoBehaviour tiedMonoBehaviour;
		private readonly IEnumerator enumCopy;
		public event Action BeforeActions;
		private bool invokedBefore = false;
		public event Action AfterActions;
		private bool invokedAfter = false;
		public bool IsRunning { get; private set; }
		public CoroutineWrapper(MonoBehaviour tiedMonoBehaviour, IEnumerator enumerator)
		{
			this.tiedMonoBehaviour = tiedMonoBehaviour;
			enumCopy = enumerator;
			BeforeActions += () => IsRunning = true;
			BeforeActions += () => invokedBefore = true;
			AfterActions += () => IsRunning = false;
			AfterActions += () => invokedAfter = true;
		}
		public CoroutineWrapper Start()
		{
			if (!invokedBefore)
				BeforeActions.Invoke();
			coroutineData = tiedMonoBehaviour.StartCoroutine(Wrapper(enumCopy));
			return this;
		}
		public void Stop()
		{
			if (IsRunning)
				tiedMonoBehaviour.StopCoroutine(coroutineData);
			if (!invokedAfter)
				AfterActions.Invoke();
		}

		private IEnumerator Wrapper(IEnumerator enumerator)
		{
			BeforeActions.Invoke();
			IsRunning = true;
			bool hasMovedNext = true;
			object output = null;
			while (hasMovedNext)
			{
				try
				{
					if (hasMovedNext = enumerator.MoveNext())
						output = enumerator.Current;
				}
				catch
				{
					StopPoint();
					throw;
				}
				if (hasMovedNext)
					yield return output;
			}
			StopPoint();
			void StopPoint()
			{
				AfterActions.Invoke();
				Stop();
			}
		}
		public void Dispose()
		{
			Stop();
		}
	}
}