namespace B1NARY
{
	using System;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Collections;
	using UnityEngine;
	using System.Collections.Generic;

	public sealed class CoroutineWrapper
	{
		/// <summary>
		/// A coroutineWrapper that expects you to use <see cref="AfterActions"/>
		/// to invoke commands.
		/// </summary>
		/// <param name="attached"> The behaviour to attach the coroutine to. </param>
		/// <param name="predicate"> The condition to wait until it results to <see langword="true"/>.</param>
		/// <returns> The wrapper to attach commands to. </returns>
		public static CoroutineWrapper WaitUntil(MonoBehaviour attached, Func<bool> predicate)
		{
			return new CoroutineWrapper(attached, WaitUntilCoroutine());
			IEnumerator WaitUntilCoroutine() { yield return new WaitUntil(predicate); }
		}
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

		/*
		
	private static Task ChangeFloat(ref float input, float final, float time)
	{
		float dynamicChange = (input - final) * -1;
		Console.WriteLine($"{(dynamicChange > 0 ? "Increasing" : "Decreasing")}, {dynamicChange}");
		Func<float, bool> isFinal = dynamicChange > 0 ? @in => @in >= final : @in => @in <= final;

		int iterations = 1;
		Stopwatch stopwatch = Stopwatch.StartNew();
		while (true)
		{
			stopwatch.Stop();
			Console.WriteLine($"Iteration: {iterations} at time {stopwatch.Elapsed.TotalSeconds}");
			float secondMultiplier = (float)stopwatch.Elapsed.TotalSeconds;
			float dynamicChange = secondMultiplier * dynamicChange / time;
			stopwatch.Restart();
			Console.WriteLine($"dynamicChange: {dynamicChange} for {input} to {input + dynamicChange}, expecting final {final}");
			input += dynamicChange;
			if (isFinal.InvokeAll(input))
				break;
			iterations++;
		}
		Console.WriteLine($"Finished with {input}");

		return Task.CompletedTask;
	}
		*//*
		

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
		public static CoroutineWrapper ChangeInteger(MonoBehaviour monoBehaviour, Ref<float> value, float finalValue, float overTime)
		{
			return new CoroutineWrapper(monoBehaviour, ModifyCoroutine()).Start();
			IEnumerator ModifyCoroutine()
			{
				float changeResult = finalValue - value;
				float deltaChangePerSecond = changeResult * overTime;
				float currentDelta = 0f;
				while (true)
				{
					yield return new WaitForFixedUpdate();
					float change = deltaChangePerSecond * Time.fixedDeltaTime;
					if (currentDelta + change > currentDelta)
					{
						change = (change - currentDelta) * -1;
						value.Value += change;
						yield break;
					}
					value.Value += change;
					currentDelta += change;
				}
			}
		}*/
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

		// General Data
		/// <summary> The copy of the enumerator. </summary>
		private readonly IEnumerator enumCopy;
		/// <summary> The place where the coroutine is tied to. </summary>
		public readonly MonoBehaviour tiedMonoBehaviour;
		public bool IsRunning { get; private set; } = false;
		private Coroutine coroutineMarker;

		// Before Actions
		public event Action<MonoBehaviour> BeforeActions;
		private bool invokedBefore = false;
		private void InvokeBeforeActions()
		{
			if (invokedBefore)
				throw new InvalidOperationException("Actions has already been invoked!");
			invokedBefore = true;
			invokedAfter = false;
			IsRunning = true;
			BeforeActions?.Invoke(tiedMonoBehaviour);
		}

		// After Actions
		public event Action<MonoBehaviour> AfterActions;
		private bool invokedAfter = false;
		private void InvokeAfterActions()
		{
			if (invokedAfter)
				throw new InvalidOperationException("Actions has already been invoked!");
			invokedAfter = true;
			invokedBefore = false;
			IsRunning = false;
			AfterActions?.Invoke(tiedMonoBehaviour);
		}

		public CoroutineWrapper Start()
		{
			if (IsRunning)
				throw new InvalidOperationException();
			InvokeBeforeActions();
			coroutineMarker = tiedMonoBehaviour.StartCoroutine(Wrapper(enumCopy));
			return this;
		}

		/// <summary>
		/// Stops the wrapper.
		/// </summary>
		/// <returns> If the object has successfully stopped. </returns>
		public bool Stop()
		{
			if (!IsRunning)
				return false;
			tiedMonoBehaviour.StopCoroutine(coroutineMarker);
			InvokeAfterActions();
			return true;
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
			bool hasMovedNext = true;
			object yieldInstruction;
			while (true)
			{
				try
				{
					hasMovedNext = enumerator.MoveNext();
					if (hasMovedNext == false)
					{
						FinishedCoroutine();
						yield break;
					}
					yieldInstruction = enumerator.Current;
				}
				catch
				{
					FinishedCoroutine();
					throw;
				}
				yield return yieldInstruction;
			}
			void FinishedCoroutine()
			{
				InvokeAfterActions();
			}
		}
	}
}