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
		protected bool CreatePanelBehaviour(GameObject confirmPanel)
		{
			if (confirmPanel == null)
				return false;
			GameObject instance = Instantiate(confirmPanel, transform);
			Button[] buttons = instance.GetComponentsInChildren<Button>();
			Button confirmButton = buttons.FirstOrDefault(button => button.name == confirmButtonName);
			if (confirmButton == null)
			{
				Debug.LogException(new NullReferenceException($"{instance} does not contain a button sub-gameobject named '{confirmButtonName}'!"));
				Destroy(instance);
				return false;
			}
			Button cancelButton = buttons.FirstOrDefault(button => button.name == cancelButtonName);
			if (cancelButton == null)
			{
				Debug.LogException(new NullReferenceException($"{instance} does not contain a button sub-gameobject named '{cancelButtonName}'!"));
				Destroy(instance);
				return false;
			}
			confirmButton.onClick.AddListener(() => OnButtonClicked.Invoke(true));
			cancelButton.onClick.AddListener(() => OnButtonClicked.Invoke(false));
			return true;
		}

		public const int saveSlotMax = 49;
		public const string cancelButtonName = "Cancel",
			confirmButtonName = "Confirm";
		[Tooltip("Should contain a button named '" + cancelButtonName + "' and '" + confirmButtonName + "'")]
		public GameObject confirmPanel;
		public UnityEvent<bool> OnButtonClicked;
		public Action<BlockInfo, bool> buttonClicked;
		protected List<BlockInfo> objects;
		public List<BlockInfo> GetSaves()
		{
			var block = new List<BlockInfo>(SaveSlot.AllFiles.Count);
			for (int i = 0; i < SaveSlot.AllFiles.Count; i++)
			{
				try
				{
					var info = new BlockInfo(AddEntry(), SaveSlot.AllFiles[i]);
					info.SetSprite(SaveSlot.AllFiles[i].ImageTexture);
					info.foregroundImage.preserveAspect = true;
					info.Text = SaveSlot.AllFiles[i].UserContents;
					if (!CreatePanelBehaviour(confirmPanel))
						info.button.onClick.AddListener(() => buttonClicked.Invoke(info, true));
					block.Add(info);
				} catch (Exception ex)
				{
					Debug.LogException(ex);
				}
			}
			return block;
		}
		public Texture2D plus;

		protected override void Awake()
		{
			base.Awake();
			buttonClicked = (info, active) => OnButtonClicked.Invoke(active);
		}
		protected virtual void OnEnable()
		{
			objects = GetSaves();
			buttonClicked += (blockInfo, confirm) =>
			{
				if (!confirm)
					return;
				blockInfo.fileData.Serialize();
				OnDisable();
				OnEnable();
			};

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
			objects?.Clear();
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
			panel.confirmPanel = DirtyAuto.Field(target, new GUIContent("Confirm Panel"), panel.confirmPanel, true);
			if (UsePlus)
				DirtyAuto.Property(serializedObject, nameof(SavePanel.plus));
			panel.objectsPerRow = DirtyAuto.Slider(target, new GUIContent("Columns"), panel.objectsPerRow, 1, 6);
		}
	}
}
#endif