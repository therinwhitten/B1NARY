namespace B1NARY
{
	using System;
	using System.Collections;
	using System.Linq;
	using System.Threading;
	using System.Threading.Tasks;
	using UnityEngine;


	public static class CoroutineUtilities
	{
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
			if (isFinal.Invoke(input))
				break;
			iterations++;
		}
		Console.WriteLine($"Finished with {input}");

		return Task.CompletedTask;
	}
		*/

		

		/// <summary>
		/// Uses a <see cref="MonoBehaviour"/> to dynamicChange a float value over time.
		/// </summary>
		/// <remarks>
		/// Using this command does not hold up the code 'path' or block.
		/// </remarks>
		/// <param name="monoBehaviour">The monoBehaviour to dynamicChange it over time. </param>
		/// <param name="value">The <see cref="float"/> value to dynamicChange, as a reference. </param>
		/// <param name="final">The final expected value. </param>
		/// <param name="secondsTaken">The seconds taken to reach to that <paramref name="final"/> value.</param>
		/// <param name="action"> An HandlePress to perform after it is finished. </param>
		public static void ChangeFloat(this MonoBehaviour monoBehaviour,
			Ref<float> value, float final, float secondsTaken, Action action = null)
		{
			float difference = (value.Value - final) * -1;
			Func<float, bool> condition = GetCondition(final, difference);
			monoBehaviour.StartCoroutine(Coroutine());

			IEnumerator Coroutine()
			{
				if (!float.IsNaN(value.Value))
					while (true)
					{
						float dynamicChange = (Time.deltaTime / secondsTaken) * difference;
						value.Value += dynamicChange;
						//Debug.Log($"New: {value.Value}, dynamicChange: {dynamicChange}, desired: {final}, isFinal: {condition.Invoke(value.Value)}");
						if (condition.Invoke(value.Value))
							break;
						yield return new WaitForEndOfFrame();
					}
				value.Value = final;
				action?.Invoke();
			}

		}

		/// <summary>
		/// Adds awaitable coroutines.
		/// </summary>
		/// <remarks>
		/// This allows the coroutine to be waited via the <see cref="Task"/> output.
		/// </remarks>
		/// <param name="monoBehaviour">The mono behaviour to run the <paramref name="monoBehaviour"/>.</param>
		/// <param name="coroutine">The coroutine to run. </param>
		/// <param name="cancellationToken">The cancellation token to stop it abruptly.</param>
		public static async Task StartAwaitableCoroutine(this MonoBehaviour monoBehaviour,
			IEnumerator coroutine)
		{
			// Creates an instance of taskCompletionSource; This uses the WaitingForActivation
			// - that is found in tasks. Unfortunately, my IDE doesn't tell me that
			// - there is a typeless taskCompletionSource
			// https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.taskcompletionsource-1.task?view=net-6.0
			//var taskCompletionSource = new TaskCompletionSource<object>();
			// Sets the value to tell it to stop waiting for activation
			bool completed = false;
			Coroutine activeCoroutine = monoBehaviour.StartCoroutine(Encapsulator(coroutine));
			await Task.Run(() => SpinWait.SpinUntil(() => completed));
			if (activeCoroutine != null)
				monoBehaviour.StopCoroutine(activeCoroutine);

			IEnumerator Encapsulator(IEnumerator child)
			{
				while (child.MoveNext())
					yield return child.Current;
				completed = true;
				yield break;
			}
		}

		private static Func<float, bool> GetCondition(float final, float change)
			=> change > 0 ?
			(Func<float, bool>)(input => IsGreaterThan(input, final)) :
			(Func<float, bool>)(input => IsLessThan(input, final));
		private static bool IsGreaterThan(float currentValue, float comparer)
			=> currentValue >= comparer;
		private static bool IsLessThan(float currentValue, float comparer)
			=> currentValue <= comparer;
	}
}