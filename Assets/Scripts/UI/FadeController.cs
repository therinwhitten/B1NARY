using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class FadeController : Multiton<FadeController>
{
	private CanvasGroup canvas;
	public float fadeSpeed = 0.1f;
	private Ref<float> FadeRef => new Ref<float>(() => fadeSpeed, set => fadeSpeed = set);

	// Start is called before the first frame update
	protected override void SingletonStart()
	{
		canvas = gameObject.GetComponent<CanvasGroup>();
	}

	public void FadeIn() => ChangeFloat(FadeRef, 1, fadeSpeed);
	public void FadeOut() => ChangeFloat(FadeRef, 0, fadeSpeed);

	// We need a custom reference for the float due to anonymous methods issue
	// - not allowing ref for value.
	public void ChangeFloat(Ref<float> value, float final, float secondsTaken)
	{
		float difference = value.Value - final;
		Func<float, float, bool> condition = difference > 0 ?
			(Func<float, float, bool>)((current, final2) => current > final2) :
			(Func<float, float, bool>)((current, final2) => current < final2);
		StartCoroutine(Coroutine(final));
		IEnumerator Coroutine(float finalValue)
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
		}
	}
}
