namespace B1NARY.UI
{
	using B1NARY.Scripting;
	using DataPersistence;
	using System.Collections.Generic;
	using System.Linq;
	using TMPro;
	using UnityEngine;
	using UnityEngine.Rendering;
	using UnityEngine.UI;

	public class SavePanel : AutoPagePopulator
	{
		public const int saveSlotMax = 49;
		protected List<BlockInfo> objects;
		public List<BlockInfo> GetSaves()
		{
			var block = new List<BlockInfo>(SaveSlot.AllFiles.Count);
			for (int i = 0; i < SaveSlot.AllFiles.Count; i++)
			{
				var info = new BlockInfo(AddEntry(), SaveSlot.AllFiles[i]);
				info.SetSprite(SaveSlot.AllFiles[i].about.ImageTexture);
				info.foregroundImage.preserveAspect = true;
				info.Text = $"\"{ScriptHandler.ToVisual(SaveSlot.AllFiles[i].scriptPosition.documentPath.FullName)}\" : {SaveSlot.AllFiles[i].data.PlayerName}\n{SaveSlot.AllFiles[i].about.lastSaved}\n{SaveSlot.AllFiles[i].about.timePlayed}";
				block.Add(info);
			}
			return block;
		}
		[SerializeField] private Texture2D plus;

		protected virtual void OnEnable()
		{
			objects = GetSaves();
			for (int i = 0; i < objects.Count; i++)
			{
				int index = i;
				objects[i].button.onClick.AddListener(() =>
				{
					SaveSlot.SaveGame(objects[index].fileData.about.fileName);
					OnDisable();
					OnEnable();
				});
			}

			if (objects.Count < saveSlotMax)
			{
				objects.Add(new BlockInfo(AddEntry(), SaveSlot.Instance));
				BlockInfo LastInfo() => objects[objects.Count - 1];
				LastInfo().button.onClick.AddListener(() =>
				{
					SaveSlot.SaveGame(SaveSlot.AllFiles.Count);
					OnDisable();
					OnEnable();
				});
				LastInfo().SetSprite(plus);
			}
		}
		protected virtual void OnDisable()
		{
			Reset();
			objects.Clear();
		}
	}
	public class BlockInfo
	{
		public readonly SaveSlot fileData;
		public readonly GameObject obj;
		public readonly Image foregroundImage;
		public readonly TMP_Text tmpText;
		public readonly Button button;
		public string Text { get => tmpText.text; set => tmpText.text = value; }
		public bool PreserveAspect { get => foregroundImage.preserveAspect; set => foregroundImage.preserveAspect = value; }
		public Sprite Sprite { get => foregroundImage.sprite; set => foregroundImage.sprite = value; }
		public BlockInfo(GameObject obj, SaveSlot fileData)
		{
			this.fileData = fileData;
			this.obj = obj;
			foregroundImage = obj.transform.Find("Foreground").GetComponent<Image>();
			tmpText = obj.GetComponentInChildren<TMP_Text>();
			button = obj.GetComponentInChildren<Button>();
		}
		public Sprite SetSprite(Texture2D texture)
		{
			return foregroundImage.sprite = Sprite.Create(texture, new Rect(Vector2.zero, new Vector2(texture.width, texture.height)), new Vector2(0.5f, 0.5f));
		}
	}
}