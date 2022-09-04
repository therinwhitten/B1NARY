namespace B1NARY
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Threading.Tasks;
	using UnityEngine;

	public class CoroutineWrapper : IEnumerable<YieldInstruction>, IDisposable
	{
		public static IEnumerator<YieldInstruction> Specify(IEnumerator enumerator)
		{
			while (enumerator.MoveNext())
				if (enumerator.Current is YieldInstruction yieldInstruction)
					yield return yieldInstruction;
				else
					throw new InvalidCastException();
		}

		private IEnumerator<YieldInstruction> activeCoroutine;
		private Coroutine monobehaviourCoroutine;
		private MonoBehaviour tiedMonobehaviour;

		public bool IsRunning { get; private set; } = false;
		public CoroutineWrapper(IEnumerator<YieldInstruction> coroutineInstruction)
		{
			activeCoroutine = coroutineInstruction;
		}
		public CoroutineWrapper(IEnumerator coroutineInstruction) 
			: this(Specify(coroutineInstruction))
		{
			
		}

		public CoroutineWrapper ContinueWith(IEnumerator<YieldInstruction> coroutineInstruction)
		{
			if (IsRunning)
				throw new InvalidOperationException(nameof(CoroutineWrapper)
					+ " will go out of sync if is edited while being used!");
			activeCoroutine = Wrapper(activeCoroutine);
			return this;
			IEnumerator<YieldInstruction> Wrapper(IEnumerator<YieldInstruction> oldBehaviour)
			{
				while (oldBehaviour.MoveNext())
					yield return oldBehaviour.Current;
				while (coroutineInstruction.MoveNext())
					yield return coroutineInstruction.Current;
			}
		}
		public CoroutineWrapper ContinueWith(IEnumerator coroutineInstruction)
			=> ContinueWith(Specify(coroutineInstruction));
		public CoroutineWrapper ContinueWith(Action action)
		{
			if (IsRunning)
				throw new InvalidOperationException(nameof(CoroutineWrapper)
					+ " will go out of sync if is edited while being used!");
			activeCoroutine = ActionWrapper(activeCoroutine);
			return this;
			IEnumerator<YieldInstruction> ActionWrapper(IEnumerator<YieldInstruction> oldBehaviour)
			{
				while (oldBehaviour.MoveNext())
					yield return oldBehaviour.Current;
				action.Invoke();
			}
		}
		public CoroutineWrapper ContinueWith(Func<Task> taskAction)
		{
			if (IsRunning)
				throw new InvalidOperationException(nameof(CoroutineWrapper)
					+ " will go out of sync if is edited while being used!");
			activeCoroutine = TaskWrapper(activeCoroutine);
			return this;
			IEnumerator<YieldInstruction> TaskWrapper(IEnumerator<YieldInstruction> oldBehaviour)
			{
				while (oldBehaviour.MoveNext())
					yield return oldBehaviour.Current;
				Task taskAwaitable = taskAction.Invoke();
				while (!taskAwaitable.IsCanceled || !taskAwaitable.IsCompleted || !taskAwaitable.IsFaulted)
					yield return new WaitForEndOfFrame();
				if (taskAwaitable.IsFaulted)
					throw taskAwaitable.Exception;
			}
		}
		public CoroutineWrapper WaitUntil(Func<bool> check)
		{
			if (IsRunning)
				throw new InvalidOperationException(nameof(CoroutineWrapper)
					+ " will go out of sync if is edited while being used!");
			activeCoroutine = WaitWrapper(activeCoroutine);
			return this;
			IEnumerator<YieldInstruction> WaitWrapper(IEnumerator<YieldInstruction> oldBehaviour)
			{
				while (oldBehaviour.MoveNext())
					yield return oldBehaviour.Current;
				while (check.Invoke() == false)
					yield return new WaitForEndOfFrame();
			}
		}
		public Coroutine Start(MonoBehaviour monoBehaviour)
		{
			IsRunning = true;
			tiedMonobehaviour = monoBehaviour;
			monobehaviourCoroutine = monoBehaviour.StartCoroutine(EndWrapper(activeCoroutine));
			return monobehaviourCoroutine;
			IEnumerator<YieldInstruction> EndWrapper(IEnumerator<YieldInstruction> instructions)
			{
				while (instructions.MoveNext())
					yield return instructions.Current;
				IsRunning = false;
			}
		}
		public async Task ToTask()
		{
			if (IsRunning == false)
				throw new InvalidOperationException("It has to be running first to await!");
			while (IsRunning)
				await Task.Yield();
		}

		public IEnumerator<YieldInstruction> GetEnumerator()
			=> activeCoroutine;
		IEnumerator IEnumerable.GetEnumerator()
			=> activeCoroutine;
		public void Dispose()
		{
			if (IsRunning)
			{
				if (tiedMonobehaviour != null)
					tiedMonobehaviour.StopCoroutine(monobehaviourCoroutine);
				IsRunning = false;
			}
		}
	}
}