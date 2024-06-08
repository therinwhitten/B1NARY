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
