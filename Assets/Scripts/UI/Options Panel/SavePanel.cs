namespace B1NARY.UI
{
	using B1NARY.Scripting;
	using DataPersistence;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using TMPro;
	using UnityEngine;
	using UnityEngine.Events;
	using UnityEngine.Rendering;
	using UnityEngine.UI;

	public class SavePanel : AutoPagePopulator
	{
		public const int saveSlotMax = 49;
		public UnityEvent OnButtonClicked;
		protected List<BlockInfo> objects;
		public List<BlockInfo> GetSaves()
		{
			var block = new List<BlockInfo>(SaveSlot.AllFiles.Count);
			for (int i = 0; i < SaveSlot.AllFiles.Count; i++)
			{
				try
				{
					var info = new BlockInfo(AddEntry(), SaveSlot.AllFiles[i]);
					info.SetSprite(SaveSlot.AllFiles[i].about.ImageTexture);
					info.foregroundImage.preserveAspect = true;
					info.Text = SaveSlot.AllFiles[i].UserContents;
					info.button.onClick.AddListener(() => OnButtonClicked.Invoke());
					block.Add(info);
				} catch (Exception ex)
				{
					Debug.LogException(ex);
				}
			}
			return block;
		}
		public Texture2D plus;

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
			Clear();
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

#if UNITY_EDITOR
namespace B1NARY.UI.Editor
{
	using B1NARY.Editor;
	using B1NARY.UI;
	using UnityEditor;
	using UnityEngine;

	[CustomEditor(typeof(SavePanel), true)]
	public class SavePanelEditor : Editor
	{
		public virtual bool UsePlus => true;
		protected SavePanel panel;
		private void Awake() => panel = (SavePanel)target;
		public override void OnInspectorGUI()
		{
			panel.slot = DirtyAuto.Field(target, new GUIContent("Slot"), panel.slot, true);
			panel.row = DirtyAuto.Field(target, new GUIContent("Row"), panel.row, true);
			if (UsePlus)
				DirtyAuto.Property(serializedObject, nameof(SavePanel.plus));
			panel.objectsPerRow = DirtyAuto.Slider(target, new GUIContent("Columns"), panel.objectsPerRow, 1, 6);
		}
	}
}
#endif