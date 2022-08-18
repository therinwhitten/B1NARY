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

		public static Task FadeInAndActivateAsync(FadeController fadeController, float fadeTime)
		{
			B1NARYConsole.Log(nameof(FadeController), "Async", $"Fading in {fadeController.name}" +
				$" with a length of {fadeTime} seconds");
			fadeController.gameObject.SetActive(true);
			fadeController.enabled = true;
			return fadeController.FadeInAsync(fadeTime);
		}
		public static void FadeInAndActivate(FadeController fadeController, float fadeTime, Action performAfter = null)
		{
			B1NARYConsole.Log(nameof(FadeController), $"Fading in {fadeController.name}" +
				$" with a length of {fadeTime} seconds");
			fadeController.gameObject.SetActive(true);
			fadeController.enabled = true;
			fadeController.FadeIn(fadeTime, performAfter);
		}

		public async Task FadeInAsync(float fadeTime)
		{
			B1NARYConsole.Log(name, "Async", $"Fading in with {fadeTime} seconds");
			await ModifyFadeFloatAsync(1, fadeTime);
			canvas.interactable = true; 
			canvas.blocksRaycasts = true;
		}
		public void FadeIn(float fadeTime, Action performAfter = null)
		{
			B1NARYConsole.Log(name, $"Fading in with {fadeTime} seconds");
			ModifyFadeFloat(1, fadeTime, () => { canvas.interactable = true; canvas.blocksRaycasts = true; performAfter?.Invoke(); });
		}

		public async Task FadeOutAsync(float fadeTime)
		{
			B1NARYConsole.Log(name, "Async", $"Fading out with {fadeTime} seconds");
			canvas.interactable = false;
			canvas.blocksRaycasts = false;
			await ModifyFadeFloatAsync(0, fadeTime);
		}
		public void FadeOut(float fadeTime, Action performAfter = null)
		{
			B1NARYConsole.Log(name, $"Fading out with {fadeTime} seconds");
			canvas.interactable = false;
			canvas.blocksRaycasts = false;
			ModifyFadeFloat(0, fadeTime, performAfter);
		}

		public async Task FadeOutAndDeActivateAsync(float fadeTime)
		{
			B1NARYConsole.Log(name, "Async", $"Fading out and de-activating with {fadeTime} seconds");
			canvas.interactable = false;
			canvas.blocksRaycasts = false;
			await ModifyFadeFloatAsync(0, fadeTime);
			gameObject.SetActive(false);
		}
		public void FadeOutAndDeActivate(float fadeTime)
		{
			B1NARYConsole.Log(name, $"Fading out and de-activating with {fadeTime} seconds");
			canvas.interactable = false;
			canvas.blocksRaycasts = false;
			ModifyFadeFloat(0, fadeTime, () => gameObject.SetActive(false));
		}



		/// <summary>
		/// Linearly modifies the canvas alpha asynchronously.
		/// </summary>
		/// <param name="final">The final expected value when finishing.</param>
		/// <param name="secondsTaken"> The seconds to take to change it linearly over time.</param>
		private Task ModifyFadeFloatAsync(float final, float secondsTaken) => 
			this.ChangeFloatAsync(new Ref<float>(() => canvas.alpha, set => canvas.alpha = set), final, secondsTaken);
		private void ModifyFadeFloat(float final, float secondsTaken, Action performAfter = null)
			=> this.ChangeFloat(new Ref<float>(() => canvas.alpha, set => canvas.alpha = set), final, secondsTaken, performAfter);
	}
}