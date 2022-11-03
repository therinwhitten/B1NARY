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