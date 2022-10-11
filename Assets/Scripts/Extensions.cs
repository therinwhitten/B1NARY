namespace B1NARY
{
	using System;
	using System.Collections;
	using System.Linq;
	using System.Threading;
	using System.Threading.Tasks;
	using UnityEngine;

	public static class ExceptionHandling
	{
		public static bool TryInvoke(this Action action)
			=> TryInvoke(action, out _);
		public static bool TryInvoke(this Action action, out Exception exception)
			=> TryInvoke<Exception>(action, out exception);
		public static bool TryInvoke<TException>(this Action action, out TException exception)
			where TException : Exception
		{
			try { action.Invoke(); }
			catch (TException e) { exception = e; return false; }
			exception = null;
			return true;
		}

		public static bool TryInvoke<TOut>(this Func<TOut> func, out TOut @out)
			=> TryInvoke(func, out @out, out _);
		public static bool TryInvoke<TOut>(this Func<TOut> func, out TOut @out, out Exception exception)
			=> TryInvoke<TOut, Exception>(func, out @out, out exception);
		public static bool TryInvoke<TOut, TException>
			(this Func<TOut> func, out TOut @out, out Exception exception) where TException : Exception
		{
			try { @out = func.Invoke(); }
			catch (TException e)
			{
				@out = default;
				exception = e;
				return false;
			}
			exception = null;
			return true;
		}

		/// <summary>
		/// Allows progression of code while providing it's own exception handler
		/// inside.
		/// </summary>
		/// <param name="task">The task to target.</param>
		public static void FreeBlockPath(this Task task)
		{
			_ = task.ContinueWith(subTask => Debug.LogException(subTask.Exception), 
				TaskContinuationOptions.OnlyOnFaulted);
		}

		public static T GetComponentWithChildren<T>(this GameObject parent) where T : UnityEngine.Object
		{
			T output = parent.GetComponent<T>();
			if (output == null)
			{
				T[] components = parent.GetComponentsInChildren<T>();
				if (components.Any())
					return components.First();
				throw new NullReferenceException($"{parent.name} does not have {typeof(T)}!");
			}
			return output;
		}
	}

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
		/// The <see cref="Task"/> allows you to tell if you want the method to wait for it to
		/// finish or not. If not, use <c>_ =</c> under it. Use <see cref="Task.Wait()"/>
		/// or <c>await</c> to wait for it to finish.
		/// </remarks>
		/// <returns>
		/// The referenced <paramref name="value"/>, should be representative of 
		/// <paramref name="final"/> once it has been returned.
		/// </returns>
		/// <param name="monoBehaviour">The monoBehaviour to dynamicChange it over time. </param>
		/// <param name="value">The <see cref="float"/> value to dynamicChange, as a reference. </param>
		/// <param name="final">The final expected value. </param>
		/// <param name="secondsTaken">The seconds taken to reach to that <paramref name="final"/> value.</param>
		/// <param name="token"> A token to cancel the float changing coroutine over time. </param>
		public static async Task ChangeFloatAsync(this MonoBehaviour monoBehaviour,
			Ref<float> value, float final, float secondsTaken, CancellationToken? token = null)
		{
			float difference = (value.Value - final) * -1;
			Func<float, bool> condition = GetCondition(final, difference);
			Func<bool> tokenCondition = token.HasValue ?
				(Func<bool>)(() => token.Value.IsCancellationRequested) : (Func<bool>)(() => true);
			bool completed = false;
			Coroutine coroutine = monoBehaviour.StartCoroutine(Coroutine());
			await Task.Run(() => SpinWait.SpinUntil(() => completed));
			monoBehaviour.StopCoroutine(coroutine);

			IEnumerator Coroutine()
			{
				if (!float.IsNaN(value.Value))
					while (tokenCondition.Invoke())
					{
						float dynamicChange = (Time.deltaTime / secondsTaken) * difference;
						value.Value += dynamicChange;
						//Debug.Log($"New: {value.Value}, dynamicChange: {dynamicChange}, desired: {final}, isFinal: {condition.Invoke(value.Value)}");
						if (condition.Invoke(value.Value))
							break;
						yield return new WaitForEndOfFrame();
					}
				value.Value = final;
				completed = true;
			}

		}

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
		/// <param name="action"> An action to perform after it is finished. </param>
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