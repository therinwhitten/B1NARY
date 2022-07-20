using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class FadeController : MonoBehaviour
{
	private CanvasGroup canvas;

	// Start is called before the first frame update
	private void Awake()
	{
		canvas = gameObject.GetComponent<CanvasGroup>();
	}


	public void FadeIn(float fadeTime)
	{
		ModifyFadeFloat(1, fadeTime, () => canvas.interactable = true);
	}

	public void FadeOut(float fadeTime)
	{
		canvas.interactable = false;
		ModifyFadeFloat(0, fadeTime);
	}


	// We need a custom reference for the float due to anonymous methods issue
	// - not allowing ref for value.
	public void ModifyFadeFloat(float final, float secondsTaken, Action performAfter = null)
	{
		float difference = canvas.alpha - final; 
		Func<float, bool> condition = difference < 0 ? 
			(Func<float, bool>)IsGreaterThan : 
			(Func<float, bool>)IsLessThan;
		difference *= -1;	// for whatever reason, this makes the code run well,
							// - I can't understand why though.
		StartCoroutine(Coroutine(final));
		IEnumerator Coroutine(float finalValue)
		{
			while (canvas.alpha != finalValue)
			{
				float change = (Time.deltaTime / secondsTaken) * difference;
				canvas.alpha += change;
				if (condition.Invoke(canvas.alpha))
				{
					canvas.alpha = finalValue;
					if (performAfter != null)
						performAfter.Invoke();
					yield break;
				}
				yield return new WaitForEndOfFrame();
			}
		}
		bool IsGreaterThan(float input) => input >= final;
		bool IsLessThan(float input) => input <= final;
	}
}
