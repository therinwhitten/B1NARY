namespace B1NARY.UI
{
	using B1NARY.Scripting;
	using DataPersistence;
	using HideousDestructor.DataPersistence;
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
		public const int saveSlotMax = 69; // nice
		public List<GameObject> panels = new List<GameObject>(3) { null, null, null };
		protected List<BlockInfo> objects;
		public GameObject slot;

		public List<BlockInfo> GetSaves()
		{
			var block = new List<BlockInfo>();
			using (var enumerator = SaveSlot.AllSlots.GetEnumerator())
				for (int i = 0; enumerator.MoveNext(); i++)
				{
					try
					{
						var info = new BlockInfo(AddEntry(slot), enumerator.Current.slot.Value);//SaveSlot.AllFiles[i]);
						info.SetSprite(enumerator.Current.slot.Value.thumbnail.Texture);
						info.foregroundImage.preserveAspect = true;
						info.Text = enumerator.Current.slot.Value.ToString();
						block.Add(info);
					} catch (Exception ex)
					{
						Debug.LogException(ex);
					}
				}
			return block;
		}
		public GameObject addPrefab;

		protected override void Awake()
		{
			base.Awake();
		}
		protected virtual void OnEnable()
		{
			objects = GetSaves();
			for (int i = 0; i < objects.Count; i++)
			{
				// Override
				BlockInfo info = objects[i];
				info.button.onClick.AddListener(() =>
				{
					var @interface = new BoxInterface(panels[1]);
					if (@interface.inputField != null)
					{
						@interface.inputField.text = info.fileData.FileLocation.NameWithoutExtension();
					}
					@interface.PressedButton += (@bool) =>
					{
						@interface.Dispose();
						if (@bool == false)
							return;
						if (@interface.inputField != null)
							info.fileData.FileLocation
							.MoveTo(info.fileData.FileLocation.FullName
								.Replace(info.fileData.FileLocation.NameWithoutExtension(), @interface.inputField.text));
						info.fileData.Save();
						SaveSlot.Refresh();
						OnDisable();
						OnEnable();
					};
				});
				// Delete
				if (info.deleteButton != null)
					info.deleteButton.onClick.AddListener(() =>
					{
						var @interface = new BoxInterface(panels[2]);
						@interface.PressedButton += (@bool) =>
						{
							@interface.Dispose();
							if (@bool == false)
								return;
							info.fileData.FileLocation.Delete();
							SaveSlot.Refresh();
							OnDisable();
							OnEnable();
						};
					});
			}

			// New slot
			if (objects.Count < saveSlotMax)
			{
				BlockInfo activeInfo = new BlockInfo(AddEntry(addPrefab), SaveSlot.ActiveSlot);
				objects.Add(activeInfo);
				activeInfo.button.onClick.AddListener(() =>
				{
					var @interface = new BoxInterface(panels[0]); 
					if (@interface.inputField != null)
					{
						@interface.inputField.text = activeInfo.fileData.FileLocation.NameWithoutExtension();
					}
					@interface.PressedButton += (@bool) =>
					{
						@interface.Dispose();
						if (@bool == false)
							return;
						if (@interface.inputField != null)
							activeInfo.fileData.FileLocation.Rename(@interface.inputField.text);
						else
							activeInfo.fileData.FileLocation = SaveSlot.ActiveSlot.FileLocation;
						activeInfo.fileData.Save();
						SaveSlot.Refresh();
						OnDisable();
						OnEnable();
					};
				});
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
		/// <summary>
		/// Possibly null if not present.
		/// </summary>
		public readonly Button deleteButton;
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
			var del = obj.transform.Find("Delete");
			if (del != null)
				deleteButton = del.GetComponentInChildren<Button>();
		}
		public Sprite SetSprite(Texture2D texture)
		{
			return foregroundImage.sprite = Sprite.Create(texture, new Rect(Vector2.zero, new Vector2(texture.width, texture.height)), new Vector2(0.5f, 0.5f));
		}
	}
	public sealed class BoxInterface : IDisposable
	{
		public const string confirmButtonName = "Confirm",
			cancelButtonName = "Cancel";
		public readonly GameObject instance;
		public event Action<bool> PressedButton;
		/// <summary>
		/// Possibly null if not present.
		/// </summary>
		public readonly TMP_InputField inputField;
		public BoxInterface(GameObject obj)
		{
			instance = obj;
			if (obj == null)
				throw new MissingReferenceException($"gameObject is null!");
			Button[] buttons = instance.GetComponentsInChildren<Button>();
			Button confirmButton = buttons.FirstOrDefault(button => button.name == confirmButtonName);
			if (confirmButton == null)
			{
				UnityEngine.Object.Destroy(instance);
				throw new NullReferenceException($"{instance} does not contain a button sub-gameobject named '{confirmButtonName}'!");
			}
			Button cancelButton = buttons.FirstOrDefault(button => button.name == cancelButtonName);
			if (cancelButton == null)
			{
				UnityEngine.Object.Destroy(instance);
				throw new NullReferenceException($"{instance} does not contain a button sub-gameobject named '{cancelButtonName}'!");
			}
			confirmButton.onClick.AddListener(() => PressedButton?.Invoke(true));
			cancelButton.onClick.AddListener(() => PressedButton?.Invoke(false));
			instance.SetActive(true);
			inputField = instance.GetComponentInChildren<TMP_InputField>();
		}

		public void Dispose()
		{
			PressedButton = null;
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
		protected SavePanel panel;
		private void Awake() => panel = (SavePanel)target;
		public override void OnInspectorGUI()
		{
			panel.slot = DirtyAuto.Field(target, new GUIContent("Slot"), panel.slot, true);
			panel.row = DirtyAuto.Field(target, new GUIContent("Row"), panel.row, true);
			panel.panels[0] = DirtyAuto.Field(target, new GUIContent("Confirm Panel"), panel.panels[0], true);
			panel.panels[1] = DirtyAuto.Field(target, new GUIContent("Overwrite Panel"), panel.panels[1], true);
			panel.panels[2] = DirtyAuto.Field(target, new GUIContent("Delete Panel"), panel.panels[2], true);
			panel.addPrefab = DirtyAuto.Field(target, new GUIContent("Adding Prefab"), panel.addPrefab, true);
			panel.objectsPerRow = DirtyAuto.Slider(target, new GUIContent("Columns"), panel.objectsPerRow, 1, 6);
		}
	}
}
#endif