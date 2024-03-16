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

		public static void FadeInAndActivate(FadeController fadeController, float fadeTime, Action performAfter = null)
		{
			Debug.Log($"Fading in {fadeController.name} with a length of " +
				$"{fadeTime} seconds");
			fadeController.gameObject.SetActive(true);
			fadeController.enabled = true;
			fadeController.FadeIn(fadeTime, performAfter);
		}
		public void FadeIn(float fadeTime, Action performAfter = null)
		{
			Debug.Log($"Fading in {name} with a length of " +
				$"{fadeTime} seconds", this);
			ModifyFadeFloat(1, fadeTime, () => { canvas.interactable = true; canvas.blocksRaycasts = true; performAfter?.Invoke(); });
		}
		public void FadeOut(float fadeTime, Action performAfter = null)
		{
			Debug.Log($"Fading out {name} with a length of " +
				$"{fadeTime} seconds", this);
			canvas.interactable = false;
			canvas.blocksRaycasts = false;
			ModifyFadeFloat(0, fadeTime, performAfter);
		}
		public void FadeOutAndDeActivate(float fadeTime)
		{
			Debug.Log($"Fading out {name} with a length of " +
				$"{fadeTime} seconds", this);
			canvas.interactable = false;
			canvas.blocksRaycasts = false;
			ModifyFadeFloat(0, fadeTime, () => gameObject.SetActive(false));
		}
		private void ModifyFadeFloat(float final, float secondsTaken, Action performAfter = null)
			=> this.ChangeFloat(new Ref<float>(() => canvas.alpha, set => canvas.alpha = set), final, secondsTaken, performAfter);
	}
}