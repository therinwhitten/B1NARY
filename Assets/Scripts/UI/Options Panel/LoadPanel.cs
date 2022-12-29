namespace B1NARY.UI
{
	using B1NARY.DataPersistence;
	using System;
	using System.Linq;

	public class LoadPanel : SavePanel
	{
		protected override void OnEnable()
		{
			base.Awake();
			objects = GetSaves().ToList();
			objects.ForEach(info => info.button.onClick.AddListener(() => SaveSlot.LoadGame(info.fileData.about.fileName)));
		}
	}
}