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
		public static HashSet<string> GalleryFlags
		{
			get
			{
				if (_galleryFlags is not null)
					return _galleryFlags;
				PlayerConfig.Instance.collectibles.MergeSavesFromSingleton();
				_galleryFlags = PlayerConfig.Instance.collectibles.Gallery;
				return _galleryFlags;
			}
		}
		private static HashSet<string> _galleryFlags;

		private CanvasGroup _canvasGroup;
		public CanvasGroup CanvasGroup => _canvasGroup == null ? _canvasGroup = GetComponent<CanvasGroup>() : _canvasGroup;

		public string FlagName = "PlaceholderFlagName";
		private void OnEnable()
		{
			UpdateCanvasGroup();
		}
		private void OnDestroy()
		{
			_galleryFlags = null;
		}

		// Has to be a separate togglable gameobject to avoid same-sync loading times
		public void UpdateCanvasGroup()
		{
			bool hide = GalleryFlags.Contains(FlagName);
			CanvasGroup.blocksRaycasts = !hide;
			CanvasGroup.alpha = hide ? 0f : 1f;
			CanvasGroup.interactable = !hide;
		}
	}
}