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

		public string FlagName = "GalleryFlag";
		private void OnEnable()
		{
			UpdateCanvasGroup();
		}

		public void UpdateCanvasGroup()
		{
			bool hide = PlayerConfig.Instance.collectibles.Gallery.Contains(FlagName);
			CanvasGroup.blocksRaycasts = !hide;
			CanvasGroup.alpha = hide ? 0f : 1f;
			CanvasGroup.interactable = !hide;
		}
	}
}
