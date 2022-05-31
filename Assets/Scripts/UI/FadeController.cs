using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeController : SingletonNew<FadeController>
{

	private CanvasGroup canvas;
	public bool fadingOut = false;
	public bool fadingIn = false;
	public float fadeSpeed = 0.1f;
	// Start is called before the first frame update
	protected override void SingletonStart()
	{
		canvas = gameObject.GetComponent<CanvasGroup>();
	}

	// Update is called once per frame
	void Update()
	{
		if (fadingOut && canvas.alpha > 0)
		{
			canvas.alpha = Mathf.MoveTowards(canvas.alpha, 0, fadeSpeed);
		}
		if (canvas.alpha == 0)
		{
			fadingOut = false;
		}
		if (fadingIn && canvas.alpha < 1)
		{
			canvas.alpha = Mathf.MoveTowards(canvas.alpha, 1, fadeSpeed);
		}
		if (canvas.alpha == 1)
		{
			fadingIn = false;
			canvas.interactable = true;
		}


	}
	public void fadeIn()
	{
		fadingIn = true;
	}
	public void fadeOut()
	{
		canvas.interactable = false;
		fadingOut = true;
	}

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
			while (true)
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
