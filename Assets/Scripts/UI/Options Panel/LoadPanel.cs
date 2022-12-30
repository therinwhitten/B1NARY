#undef SAVE_PANEL

namespace B1NARY.UI
{
	using B1NARY.DataPersistence;
	using System;
	using System.Linq;
	using UnityEngine;

	public class LoadPanel : SavePanel
	{
		protected override void OnEnable()
		{
			objects = GetSaves();
			for (int i = 0; i < objects.Count; i++)
			{
				int index = i;
				objects[i].button.onClick.AddListener(() =>
				{
					SaveSlot.LoadGame(objects[index].fileData.about.fileName);
				});
			}
		}
	}
}