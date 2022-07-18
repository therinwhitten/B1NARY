using System;
using System.Collections;
using UnityEngine;

public static class Extensions
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


	public static void ChangeFloat(this MonoBehaviour monoBehaviour, 
		Ref<float> value, float final, float secondsTaken)
	{
		float difference = value.Value - final;
		Func<float, bool> condition = difference < 0 ?
			(Func<float, bool>)IsGreaterThan :
			(Func<float, bool>)IsLessThan;
		difference *= -1;   // for whatever reason, this makes the code run well,
							// - I can't understand why though.
		monoBehaviour.StartCoroutine(Coroutine(final));
		IEnumerator Coroutine(float finalValue)
		{
			while (value.Value != finalValue)
			{
				if (float.IsNaN(value.Value))
				{
					value.Value = finalValue;
					yield break;
				}
				float change = (Time.deltaTime / secondsTaken) * difference;
				value.Value += change;
				if (condition.Invoke(value.Value))
				{
					value.Value = finalValue;
					yield break;
				}
				yield return new WaitForEndOfFrame();
			}
		}
		bool IsGreaterThan(float input) => input >= final;
		bool IsLessThan(float input) => input <= final;
	}


	public static void Log(this object obj) => Debug.Log(obj);
	public static void LogWarn(this object obj) => Debug.LogWarning(obj);
	public static void LogErr(this object obj) => Debug.LogError(obj);
}