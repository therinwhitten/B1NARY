namespace B1NARY.UI
{
	using B1NARY.DataPersistence;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using UnityEngine;

	[RequireComponent(typeof(CanvasGroup))]
	public class LockBoxToggler : MonoBehaviour
	{
		private CanvasGroup _canvasGroup;
		public CanvasGroup CanvasGroup => _canvasGroup == null ? _canvasGroup = GetComponent<CanvasGroup>() : _canvasGroup;

		public string FlagName = "PlaceholderFlagName";
		private void OnEnable()
		{
			UpdateCanvasGroup();
		}

		// Has to be a separate togglable gameobject to avoid same-sync loading times
		public void UpdateCanvasGroup()
		{
			bool hide = FanartPanel.Instance.GalleryFlags.Contains(FlagName);
			CanvasGroup.blocksRaycasts = !hide;
			CanvasGroup.alpha = hide ? 0f : 1f;
			CanvasGroup.interactable = !hide;
		}
	}
}
