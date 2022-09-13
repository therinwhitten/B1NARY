namespace B1NARY
{
	/*
	using System;
	using System.Collections;
	using UnityEngine;
	using Live2D.Cubism.Framework.Expression;
	using Live2D.Cubism.Rendering;
	using B1NARY.Scripting.Experimental;
	using B1NARY.Audio;

	[RequireComponent(typeof(Animator)), Obsolete]
	public class CharScriptOld : MonoBehaviour
	{
		private Animator animator;

		[HideInInspector]
		public string currentExpression;
		[HideInInspector]
		public string prefabName;

		[HideInInspector]
		public string currentAnimation = "idle";

		private string defaultAnimation = "idle";
		public string charName;
		public string[] expressions;
		[HideInInspector]
		public RectTransform rectTransform;
		public AudioSource voice;
		public CubismRenderer[] renderers;
		private Material lighting;
		private Material lightingFocus;
		private Material lightingNoFocus;
		public bool focused = false;

		public Vector2 anchorPadding { get { return rectTransform.anchorMax - rectTransform.anchorMin; } }
		private Vector3 originalScale;
		float voicevolume = 1f;
		private void Awake()
		{
			rectTransform = gameObject.GetComponent<RectTransform>();
			originalScale = rectTransform.localScale;
			voice = gameObject.GetComponent<AudioSource>();
			renderers = gameObject.GetComponentsInChildren<CubismRenderer>();
			animator = GetComponent<Animator>();
			// initLighting();
		}
		public void lightingIntoFocus()
		{
			focused = true;
			targetMaterial = lighting;
			targetScale = new Vector3(originalScale.x * 1.05f, originalScale.y * 1.05f, originalScale.z);
			transitionFocus();
		}
		public void lightingOutOfFocus()
		{
			focused = false;
			targetMaterial = lightingNoFocus;
			targetScale = new Vector3(originalScale.x * 0.95f, originalScale.y * 0.95f, originalScale.z);
			transitionFocus();
		}

		private void stopLighting()
		{
			if (transitioningFocus != null)
			{
				StopCoroutine(transitioningFocus);
				transitioningFocus = null;
			}
		}
		private void transitionFocus()
		{
			stopLighting();
			transitioningFocus = StartCoroutine(TransitioningFocus());
		}
		Coroutine transitioningFocus = null;
		Material targetMaterial;
		Vector3 targetScale;
		IEnumerator TransitioningFocus()
		{
			changeLighting(targetMaterial);

			while (transform.localScale != targetScale)
			{
				transform.localScale = Vector3.MoveTowards(rectTransform.localScale, targetScale, 5f);
				yield return new WaitForEndOfFrame();
			}
			transitioningFocus = null;
		}
		public void initLighting()
		{
			lighting = new Material(GameObject.Find("LightingOverlay").GetComponent<SpriteRenderer>().material);

			lightingFocus = new Material(lighting);
			lightingFocus.color = new Color(lighting.color.r + lighting.color.r / 4, lighting.color.g + lighting.color.g / 4, lighting.color.b + lighting.color.b / 4);

			lightingNoFocus = new Material(lighting);
			lightingNoFocus.color = new Color(lighting.color.r - lighting.color.r / 2, lighting.color.g - lighting.color.g / 2, lighting.color.b - lighting.color.b / 2);

			foreach (CubismRenderer renderer in renderers)
			{
				if (lighting != null)
				{
					renderer.Material = lighting;
				}
			}
		}
		private void changeLighting(Material material)
		{
			if (material == null)
				return;
			foreach (CubismRenderer renderer in renderers)
				renderer.Material = material;
		}
	}
	*/
}