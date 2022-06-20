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
		Func<float, float, bool> condition = difference > 0 ?
			(Func<float, float, bool>)((current, final2) => current > final2) :
			(Func<float, float, bool>)((current, final2) => current < final2);
		monoBehaviour.StartCoroutine(Coroutine(final));
		IEnumerator Coroutine(float finalValue, Action action = null)
		{
			while (value.Value != finalValue)
			{
				float change = (Time.deltaTime / secondsTaken) * difference;
				if (condition.Invoke(value.Value, finalValue))
				{
					value.Value = finalValue;
					yield break;
				}
				else
					value.Value += change;
				yield return new WaitForEndOfFrame();
			}
			action?.Invoke();
		}
	}

	/// <summary>
	///		Sets the input float value to a random variator ranging from 0 to 1,
	///		the current input float value is the max while the variance decreases
	///		that value.
	/// </summary>
	/// <param name="input">The input value.</param>
	/// <param name="varianceMult">Where the input value is multiplied from randomly.</param>
	/// <returns>The modified input value.</returns>
	public static float ApplyRandomPercent(this float input, float varianceMult, RandomFowarder.RandomType randomType)
	{
		if (varianceMult == 0)
			return input;
		float baseValue = input * varianceMult,
			subValue = input - baseValue;
		return baseValue + (RandomFowarder.NextFloat(randomType) * subValue);
	}
}