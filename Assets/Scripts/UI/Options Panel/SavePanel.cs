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
		public IEnumerable<BlockInfo> GetSaves()
		{
			return SaveSlot.AllFiles.Select(data => new BlockInfo(AddEntry(), data)
			{
				PreserveAspect = true,
				Sprite = Create(data.about.ImageTexture),
				Text = $"\"{ScriptHandler.ToVisual(data.scriptPosition.documentPath)}\" : {data.data.PlayerName}\n{data.about.lastSaved}\n{data.about.timePlayed}",
			});
			Sprite Create(Texture2D texture2D)
			{
				return Sprite.Create(texture2D, new Rect(Vector2.zero, new Vector2(texture2D.width, texture2D.height)), new Vector2(0.5f, 0.5f));
			}
		}
		[SerializeField] private Texture2D plus;

		protected virtual void OnEnable()
		{
			base.Awake();
			objects = GetSaves().ToList();
			objects.ForEach(block => block.button.onClick.AddListener(() => ListenerCallbacker(block.fileData.about.fileName)));
			if (objects.Count < saveSlotMax)
			{
				objects.Add(new BlockInfo(AddEntry(), SaveSlot.Instance));
				BlockInfo LastInfo() => objects[objects.Count - 1];
				LastInfo().button.onClick.AddListener(() =>
				{
					SaveSlot.SaveGame(objects.Count);
					OnDisable();
					OnEnable();
				});
				LastInfo().SetSprite(plus);
			}
			

			void ListenerCallbacker(string name)
			{
				SaveSlot.SaveGame(name);
				OnDisable();
				OnEnable();
			}
		}
		protected virtual void OnDisable()
		{
			Reset();
			objects.Clear();
		}
	}
	public readonly struct BlockInfo
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