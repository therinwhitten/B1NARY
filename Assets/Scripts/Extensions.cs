using System;
using System.Collections;
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

}

public static class CoroutineUtilities
{
	/// <summary>
	/// Uses a <see cref="MonoBehaviour"/> to change a float value over time.
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
	/// <param name="monoBehaviour">The monoBehaviour to change it over time. </param>
	/// <param name="value">The <see cref="float"/> value to change, as a reference. </param>
	/// <param name="final">The final expected value. </param>
	/// <param name="secondsTaken">The seconds taken to reach to that <paramref name="final"/> value.</param>
	public static async Task ChangeFloat(this MonoBehaviour monoBehaviour, 
		Ref<float> value, float final, float secondsTaken, CancellationToken? token = null)
	{
		float difference = value.Value - final;
		Func<float, bool> condition = difference < 0 ?
			(Func<float, bool>)IsGreaterThan :
			(Func<float, bool>)IsLessThan;
		difference *= -1;   // for whatever reason, this makes the code run well,
							// - I can't understand why though.
		var completedCoroutine = new TaskCompletionSource<object>();
		monoBehaviour.StartCoroutine(Coroutine(final));
		await completedCoroutine.Task;
		IEnumerator Coroutine(float finalValue)
		{
			while (!token.HasValue || !token.Value.IsCancellationRequested)
			{
				if (float.IsNaN(value.Value))
				{
					value.Value = finalValue;
					yield break;
				}
				float change = Time.unscaledDeltaTime / secondsTaken * difference;
				value.Value += change;
				if (condition.Invoke(value.Value))
				{
					value.Value = finalValue;
					yield break;
				}
				yield return new WaitForEndOfFrame();
			}
			completedCoroutine.SetResult(new object());
		}
		bool IsGreaterThan(float input) => input >= final;
		bool IsLessThan(float input) => input <= final;
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
		Func<Action, IEnumerator> coroutine)
	{
		// Creates an instance of taskCompletionSource; This uses the WaitingForActivation
		// - that is found in tasks. Unfortunately, my IDE doesn't tell me that
		// - there is a typeless taskCompletionSource
		// https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.taskcompletionsource-1.task?view=net-6.0
		var taskCompletionSource = new TaskCompletionSource<object>();
		// Sets the value to tell it to stop waiting for activation
		Action performAfter = () => taskCompletionSource.SetResult(new object());
		Coroutine activeCoroutine = monoBehaviour.StartCoroutine(coroutine(performAfter));
		await taskCompletionSource.Task;
		if (activeCoroutine != null)
			monoBehaviour.StopCoroutine(activeCoroutine);
	}

	// Extra stuff in the wrong class, but oh well
	public static void Log(this object obj) => Debug.Log(obj);
	public static void LogWarn(this object obj) => Debug.LogWarning(obj);
	public static void LogErr(this object obj) => Debug.LogError(obj);
}