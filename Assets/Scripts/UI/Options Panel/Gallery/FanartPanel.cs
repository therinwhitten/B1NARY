namespace B1NARY.UI
{
	using B1NARY.DataPersistence;
	using B1NARY.DesignPatterns;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using UnityEngine;

	public class FanartPanel : Singleton<FanartPanel>
	{
		public HashSet<string> GalleryFlags
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
		private HashSet<string> _galleryFlags;

		protected virtual void OnEnable()
		{
			SaveSlot.EmptiedSaveCache += UpdateSaves;
		}
		private void UpdateSaves()
		{
			OnDisable();
			OnEnable();
		}
		protected virtual void OnDisable()
		{
			SaveSlot.EmptiedSaveCache -= UpdateSaves;
			Clear();
		}
		public void Clear()
		{
			_galleryFlags = null;
		}
	}
}
