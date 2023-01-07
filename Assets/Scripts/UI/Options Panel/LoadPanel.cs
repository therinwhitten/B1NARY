namespace B1NARY.UI
{
	using B1NARY.DataPersistence;
	using System;
	using System.Linq;
	using UnityEngine;
	using UnityEngine.Events;

	public class LoadPanel : SavePanel
	{
		protected override void OnEnable()
		{
			base.Awake();
			objects = GetSaves().ToList();
			for (int i = 0; i < objects.Count; i++)
				objects[i].button.onClick.AddListener(() => LoadGame(objects[i].fileData.about.fileName));
		}
		public void LoadGame(string fileName) => SaveSlot.LoadGame(fileName);
	}
}
#if UNITY_EDITOR
namespace B1NARY.UI.Editor
{
	using System;
	using UnityEditor;
}
#endif