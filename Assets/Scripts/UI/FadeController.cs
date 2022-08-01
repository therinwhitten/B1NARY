namespace B1NARY.UI
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Threading.Tasks;
	using UnityEngine;

	[RequireComponent(typeof(CanvasGroup))]
	public class FadeController : MonoBehaviour
	{
		private CanvasGroup canvas;

		private void Awake()
		{
			canvas = gameObject.GetComponent<CanvasGroup>();
		}

		public static Task FadeInAndActivate(FadeController fadeController, float fadeTime)
		{
			B1NARYConsole.Log(nameof(FadeController), $"Fading in {fadeController.name}" +
				$" with a length of {fadeTime} seconds");
			fadeController.gameObject.SetActive(true);
			fadeController.enabled = true;
			return fadeController.FadeIn(fadeTime);
		}

		public async Task FadeIn(float fadeTime)
		{
			B1NARYConsole.Log(name, $"Fading in with {fadeTime} seconds");
			await ModifyFadeFloat(1, fadeTime, () => { canvas.interactable = true; canvas.blocksRaycasts = true; });
		}

		public async Task FadeOut(float fadeTime)
		{
			B1NARYConsole.Log(name, $"Fading out with {fadeTime} seconds");
			canvas.interactable = false;
			canvas.blocksRaycasts = false;
			await ModifyFadeFloat(0, fadeTime);
		}

		public async Task FadeOutAndDeActivate(float fadeTime)
		{
			B1NARYConsole.Log(name, $"Fading out and de-activating with {fadeTime} seconds");
			canvas.interactable = false;
			canvas.blocksRaycasts = false;
			await ModifyFadeFloat(0, fadeTime, () => gameObject.SetActive(false));
		}


		// We need a custom reference for the float due to anonymous methods issue
		// - not allowing ref for value.
		private async Task ModifyFadeFloat(float final, float secondsTaken, Action performAfter = null)
		{
			float difference = canvas.alpha - final;
			Func<float, bool> condition = difference < 0 ?
				(Func<float, bool>)IsGreaterThan :
				(Func<float, bool>)IsLessThan;
			difference *= -1;   // for whatever reason, this makes the code run well,
								// - I can't understand why though.
			var completionSource = new TaskCompletionSource<object>();
			StartCoroutine(Coroutine(final));
			await completionSource.Task;
			performAfter?.Invoke();

			IEnumerator Coroutine(float finalValue)
			{
				while (canvas.alpha != finalValue)
				{
					float change = (Time.deltaTime / secondsTaken) * difference;
					canvas.alpha += change;
					if (condition.Invoke(canvas.alpha))
					{
						canvas.alpha = finalValue;
						completionSource.SetResult(new object());
						yield break;
					}
					yield return new WaitForEndOfFrame();
				}
			}
			bool IsGreaterThan(float input) => input >= final;
			bool IsLessThan(float input) => input <= final;
		}
	}
}