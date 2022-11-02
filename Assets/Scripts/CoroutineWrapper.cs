namespace B1NARY
{
	using System;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Collections;
	using UnityEngine;

	public sealed class CoroutineWrapper : IDisposable
	{
		/// <summary>
		/// Changes the integer over time.
		/// </summary>
		/// <param name="monoBehaviour">
		/// Where the <see cref="CoroutineWrapper"/> will attach to. 
		/// </param>
		/// <param name="value"> The value to modify over time. </param>
		/// <param name="finalValue"> 
		/// What the value should be after <paramref name="overTime"/> has passed.
		/// </param>
		/// <param name="overTime"> 
		/// The amount of seconds to pass to change the value. 
		/// </param>
		/// <returns> 
		/// The coroutine that modifies the <paramref name="value"/> over time
		/// </returns>
		//public static CoroutineWrapper ChangeInt eger(MonoBehaviour monoBehaviour, Ref<float> value, float finalValue, float overTime)
		//{
		//
		//
		//
		//	IEnumerator ModifyCoroutine()
		//	{
		//
		//	}
		//}
		/// <summary>
		/// <see cref="true"/> if the coroutine is set as <see langword="null"/>
		/// or it is not running. Otherwise, <see cref="false"/>.
		/// </summary>
		public static bool IsNotRunningOrNull(CoroutineWrapper coroutineWrapper)
		{
			if (coroutineWrapper == null)
				return true;
			return !coroutineWrapper.IsRunning;
		}

		/// <summary>
		/// Gives a <see cref="Task"/> that will wait until <see cref="IsRunning"/>
		/// returns false, using CPU spinning.
		/// </summary>
		public Task Awaiter => Task.Run(() => SpinWait.SpinUntil(() => !IsRunning));
		/// <summary> The coroutine to reference when deleting. </summary>
		private Coroutine coroutineData = null;
		/// <summary>
		/// The <see cref="MonoBehaviour"/> to tie into for the <see cref="Coroutine"/>.
		/// </summary>
		private readonly MonoBehaviour tiedMonoBehaviour;
		/// <summary>
		/// The enumerator that the constructor defined, and can be readily used
		/// for making <see cref="coroutineData"/>.
		/// </summary>
		private readonly IEnumerator enumCopy;
		/// <summary>
		/// Actions before the coroutine starts.
		/// </summary>
		public event Action BeforeActions;
		private bool invokedBefore = false;
		private bool TryInvokeBefore(bool IgnoreTiedActions = false)
		{
			if (invokedBefore)
				return false;
			invokedBefore = true;
			IsRunning = true;
			if (!IgnoreTiedActions)
				BeforeActions.Invoke();
			return true;
		}
		/// <summary>
		/// Actions after the coroutine starts.
		/// </summary>
		public event Action AfterActions;
		private bool invokedAfter = false;
		private bool TryInvokeAfter(bool IgnoreTiedActions = false)
		{
			if (invokedAfter)
				return false;
			invokedAfter = true;
			IsRunning = false;
			if (!IgnoreTiedActions)
				AfterActions.Invoke();
			return true;
		}
		/// <summary>
		/// If it is running, .
		/// </summary>
		public bool IsRunning { get; private set; }
		/// <summary>
		/// Creates a new instance of <see cref="CoroutineWrapper"/>
		/// </summary>
		/// <param name="tiedMonoBehaviour"> 
		/// What <see cref="MonoBehaviour"/> to tie into for the <see cref="Coroutine"/>.
		/// </param>
		/// <param name="enumerator"> The coroutine enumerator itself. </param>
		public CoroutineWrapper(MonoBehaviour tiedMonoBehaviour, IEnumerator enumerator)
		{
			this.tiedMonoBehaviour = tiedMonoBehaviour;
			enumCopy = enumerator;
		}
		/// <summary>
		/// Starts the <see cref="Coroutine"/> and <see cref="CoroutineWrapper"/>.
		/// Is chainable, as to simply create it in one line and start immediately.
		/// </summary>
		/// <returns> itself. </returns>
		public CoroutineWrapper Start()
		{
			TryInvokeBefore();
			coroutineData = tiedMonoBehaviour.StartCoroutine(Wrapper(enumCopy));
			return this;
		}
		/// <summary>
		/// Forcefully stops the <see cref="Coroutine"/>, usually 'cutting' the
		/// enumeration mid-way.
		/// </summary>
		public void Stop(bool IgnoreTiedActions = false)
		{
			if (IsRunning)
				tiedMonoBehaviour.StopCoroutine(coroutineData);
			TryInvokeAfter();
		}

		/// <summary>
		/// Adds additional behaviour on before and after it plays. Contains a 
		/// built-in try-catcher that stops the <see cref="CoroutineWrapper"/>
		/// if it hits a exception, which it promptly throws afterwards.
		/// </summary>
		/// <param name="enumerator"></param>
		/// <returns> <paramref name="enumerator"/>'s value. </returns>
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