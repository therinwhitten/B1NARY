using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeController : MonoBehaviour
{
	private CanvasGroup canvas;

	private void Awake()
	{
		canvas = gameObject.GetComponent<CanvasGroup>();
	}

	public static void FadeInAndActivate(FadeController fadeController, float fadeTime)
	{
		fadeController.gameObject.SetActive(true);
		fadeController.FadeIn(fadeTime);
	}

	public void FadeIn(float fadeTime)
	{
		ModifyFadeFloat(1, fadeTime, () => { canvas.interactable = true; canvas.blocksRaycasts = true; });
	}

	public void FadeOut(float fadeTime)
	{
		canvas.interactable = false;
		canvas.blocksRaycasts = false;
		ModifyFadeFloat(0, fadeTime);
	}

	public void FadeOutAndDeActivate(float fadeTime)
	{
		canvas.interactable = false;
		canvas.blocksRaycasts = false;
		ModifyFadeFloat(0, fadeTime, () => gameObject.SetActive(false));
	}


	// We need a custom reference for the float due to anonymous methods issue
	// - not allowing ref for value.
	private void ModifyFadeFloat(float final, float secondsTaken, Action performAfter = null)
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
