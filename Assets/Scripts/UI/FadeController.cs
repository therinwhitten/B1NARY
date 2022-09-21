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
			Debug.Log($"Fading in {fadeController.name} with a length of " +
				$"{fadeTime} seconds");
			fadeController.gameObject.SetActive(true);
			fadeController.enabled = true;
			return fadeController.FadeInAsync(fadeTime);
		}
		public static void FadeInAndActivate(FadeController fadeController, float fadeTime, Action performAfter = null)
		{
			Debug.Log($"Fading in {fadeController.name} with a length of " +
				$"{fadeTime} seconds");
			fadeController.gameObject.SetActive(true);
			fadeController.enabled = true;
			fadeController.FadeIn(fadeTime, performAfter);
		}

		public async Task FadeInAsync(float fadeTime)
		{
			Debug.Log($"Fading in {name} with a length of " +
				$"{fadeTime} seconds", this);
			await ModifyFadeFloatAsync(1, fadeTime);
			canvas.interactable = true; 
			canvas.blocksRaycasts = true;
		}
		public void FadeIn(float fadeTime, Action performAfter = null)
		{
			Debug.Log($"Fading in {name} with a length of " +
				$"{fadeTime} seconds", this);
			ModifyFadeFloat(1, fadeTime, () => { canvas.interactable = true; canvas.blocksRaycasts = true; performAfter?.Invoke(); });
		}

		public async Task FadeOutAsync(float fadeTime)
		{
			Debug.Log($"Fading out {name} with a length of " +
				$"{fadeTime} seconds", this);
			canvas.interactable = false;
			canvas.blocksRaycasts = false;
			await ModifyFadeFloatAsync(0, fadeTime);
		}
		public void FadeOut(float fadeTime, Action performAfter = null)
		{
			Debug.Log($"Fading out {name} with a length of " +
				$"{fadeTime} seconds", this);
			canvas.interactable = false;
			canvas.blocksRaycasts = false;
			ModifyFadeFloat(0, fadeTime, performAfter);
		}

		public async Task FadeOutAndDeActivateAsync(float fadeTime)
		{
			Debug.Log($"Fading out {name} with a length of " +
				$"{fadeTime} seconds", this);
			canvas.interactable = false;
			canvas.blocksRaycasts = false;
			await ModifyFadeFloatAsync(0, fadeTime);
			gameObject.SetActive(false);
		}
		public void FadeOutAndDeActivate(float fadeTime)
		{
			Debug.Log($"Fading out {name} with a length of " +
				$"{fadeTime} seconds", this);
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