namespace B1NARY.UI.Colors
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using UnityEngine;
	using UnityEngine.UI;

	[DisallowMultipleComponent]
	public class UIThemeHandler : MonoBehaviour
	{

		public string imageThemeName = ColorFormat.COLOR_NAME_SECONDARY,
			buttonHighlightedName = ColorFormat.COLOR_NAME_PRIMARY,
			buttonPressedName = ColorFormat.COLOR_NAME_PRIMARY,
			buttonSelectedName = ColorFormat.COLOR_NAME_PRIMARY,
			buttonDisabledName = ColorFormat.COLOR_NAME_PRIMARY;

		public bool PointersDefined => Color != null || ColorBlock != null;
		public Ref<Color> Color { get; private set; } = null;
		public string cacheColor = null;
		public Ref<ColorBlock> ColorBlock { get; private set; } = null;
		public string cacheColorblock = null;
		internal void UpdateColors()
		{
			DefineReferences();
			if (Color != null)
			{
				Color.Value = GetColor(imageThemeName);
				return;
			}
			if (ColorBlock != null)
			{
				ColorBlock.Value = new ColorBlock
				{
					colorMultiplier = ColorBlock.Value.colorMultiplier,
					disabledColor = GetColor(buttonDisabledName),
					fadeDuration = ColorBlock.Value.fadeDuration,
					highlightedColor = GetColor(buttonHighlightedName),
					normalColor = GetColor(imageThemeName),
					pressedColor = GetColor(buttonPressedName),
					selectedColor = GetColor(buttonSelectedName),
				};
				return;
			}
			if (TargetComponent != null)
				throw new IndexOutOfRangeException($"The graphic {TargetComponent} doesn't contain a color");
			else
				throw new IndexOutOfRangeException($"Graphic component isn't assigned!");

			static Color GetColor(string key)
			{
				if (ColorFormat.ActiveFormat.TryGetColor(key, out Color color))
					return color;
				throw new NullReferenceException($"'{key}' is not located within the currently " +
					$"equipped format: {ColorFormat.ActiveFormat.FormatName}.");
			}
		}
		private void UpdateColorsDelegate(ColorFormat format)
		{
			try { UpdateColors(); } catch (Exception ex) { Debug.LogException(ex); }
		}

		internal bool CheckForColor(out PropertyInfo info)
		{
			if (TargetComponent == null)
			{
				Debug.LogWarning($"Target graphic is null!", this);
				info = null;
				return false;
			}
			Type componentType = TargetComponent.GetType();
			if (!string.IsNullOrEmpty(cacheColor))
			{
				PropertyInfo suspected = componentType.GetProperty(cacheColor, BindingFlags.Public | BindingFlags.Instance);
				if (suspected != null)
				{
					info = suspected;
					return true;
				}
				cacheColor = null;
			}
			PropertyInfo[] properties = componentType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
			for (int ii = 0; ii < properties.Length; ii++)
			{
				PropertyInfo currentProperty = properties[ii];
				if (currentProperty.PropertyType != typeof(Color))
					continue;
				cacheColor = currentProperty.Name;
				info = currentProperty;
				return true;
			}
			info = null;
			return false;
		}
		internal bool CheckForColorBlock(out PropertyInfo info)
		{
			if (TargetComponent == null)
			{
				Debug.LogWarning($"Target graphic is null!", this);
				info = null;
				return false;
			}
			Type componentType = TargetComponent.GetType();
			if (!string.IsNullOrEmpty(cacheColorblock))
			{
				PropertyInfo suspected = componentType.GetProperty(cacheColor, BindingFlags.Public | BindingFlags.Instance);
				if (suspected != null)
				{
					info = suspected;
					return true;
				}
				cacheColor = null;
			}
			PropertyInfo[] properties = componentType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
			for (int ii = 0; ii < properties.Length; ii++)
			{
				PropertyInfo currentProperty = properties[ii];
				if (currentProperty.PropertyType != typeof(ColorBlock))
					continue;
				cacheColorblock = currentProperty.Name;
				info = currentProperty;
				return true;
			}
			info = null;
			return false;
		}
		internal bool DefineReferences()
		{
			if (TargetComponent == null)
				return false;
			if (Color is not null || ColorBlock is not null)
				return true;
			if (CheckForColor(out PropertyInfo info))
			{
				Color = new Ref<Color>(() => (Color)info.GetValue(TargetComponent), set => info.SetValue(TargetComponent, set));
				return true;
			}
			if (CheckForColorBlock(out info))
			{
				ColorBlock = new Ref<ColorBlock>(() => (ColorBlock)info.GetValue(TargetComponent), set => info.SetValue(TargetComponent, set));
				return true;
			}
			return false;
		}

		public Component TargetComponent
		{
			get
			{
				//if (m_savedComponent == null)
				//	TargetComponent = GetComponent<Graphic>();
				return m_savedComponent;
			}

			set
			{
				if (value == null)
				{
					m_savedComponent = null;
					return;
				}
				Component savedGraphic = m_savedComponent;
				m_savedComponent = value;
				if (CheckForColor(out _) || CheckForColorBlock(out _))
					return;
				m_savedComponent = savedGraphic;
				Debug.LogWarning($"Couldn't save graphic {savedGraphic} as it doesn't have a {nameof(Color)} or {nameof(ColorBlock)} component!", this);
			}
		}
		[SerializeField]
		private Component m_savedComponent;

		private void Reset()
		{
			TargetComponent = GetComponent<Graphic>();
			// Serializing strings on creation
			if (CheckForColor(out _))
				CheckForColorBlock(out _);
		}

#if UNITY_EDITOR
		private void Awake()
		{
			if (TargetComponent == null)
				Debug.LogWarning($"component target is null!", this);
			if (CheckForColor(out _) == false && CheckForColorBlock(out _) == false)
				Debug.LogWarning($"'{name}' doesn't lead to a graphic with a {nameof(Color)} or {nameof(ColorBlock)} properties!\n" +
					$"This message will only appear in the editor; Click on the error to easily locate the source.", this);
		}
#endif
		private void Start()
		{
			if (TargetComponent == null)
			{
				Debug.LogWarning($"Target graphic is null!", this);
				return;
			}
			UpdateColors();
		}
		private void OnEnable()
		{
			ColorFormat.ChangedFormat += UpdateColorsDelegate;
		}
		private void OnDisable()
		{
			ColorFormat.ChangedFormat -= UpdateColorsDelegate;
		}

	}
}
#if UNITY_EDITOR
namespace B1NARY.UI.Colors.Editor
{
	using B1NARY.Editor;
	using DG.DemiEditor;
	using DG.Tweening;
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.IO;
	using System.Linq;
	using System.Reflection;
	using System.Text;
	using UnityEditor;
	using UnityEngine;
	using UnityEngine.UI;

	[CustomEditor(typeof(UIThemeHandler))]
	public class UIThemeHandlerEditor : Editor
	{
		private static readonly ReadOnlyCollection<string> defaultValues = 
			new(new string[]
			{
				ColorFormat.COLOR_NAME_PRIMARY,
				ColorFormat.COLOR_NAME_SECONDARY
			});
		private static List<string> GetAllColorNames()
		{
			var hashSet = new HashSet<string>();
			var list = new List<string>(defaultValues);
			IReadOnlyList<ColorFormat> allFormats = ColorFormat.GetCombinedAllFormats();
			for (int i = 0; i < allFormats.Count; i++)
			{
				string[] allKeys = new string[allFormats[i].ExtraUIColors.Count];
				allFormats[i].ExtraUIColors.Keys.CopyTo(allKeys, 0);
				for (int ii = 0; ii < allKeys.Length; ii++)
					if (!hashSet.Contains(allKeys[ii]))
					{
						list.Add(allKeys[ii]);
						hashSet.Add(allKeys[ii]);
					}
			}
			return list;
		}

		UIThemeHandler currentHandler;
		private void Awake() => currentHandler = (UIThemeHandler)target;

		public override void OnInspectorGUI()
		{
			//if (GUILayout.Button("Update Prefabs"))
			//	TestSwitch();
			currentHandler.TargetComponent = DirtyAuto.Field(target, new("Target Component"), currentHandler.TargetComponent, true);
			if (!currentHandler.DefineReferences())
			{
				if (currentHandler.TargetComponent == null)
					return;
				else if (currentHandler.Color is null && currentHandler.ColorBlock is null)
					EditorGUILayout.HelpBox("The graphic has no customizable colors", MessageType.Warning);
				else
					EditorGUILayout.HelpBox("what is going on", MessageType.Warning);
				return;
			}
			// Update variables

			EditorGUILayout.Space();

			if (currentHandler.Color != null)
			{
				ModifyColor("Color Option", new Ref<string>(() => currentHandler.imageThemeName, str => currentHandler.imageThemeName = str));
			}
			else if (currentHandler.ColorBlock != null)
			{
				ModifyColor("Normal Color", new Ref<string>(() => currentHandler.imageThemeName, str => currentHandler.imageThemeName = str));
				ModifyColor("Highlighted Color", new Ref<string>(() => currentHandler.buttonHighlightedName, str => currentHandler.buttonHighlightedName = str));
				ModifyColor("Pressed Color", new Ref<string>(() => currentHandler.buttonPressedName, str => currentHandler.buttonPressedName = str));
				ModifyColor("Selected Color", new Ref<string>(() => currentHandler.buttonSelectedName, str => currentHandler.buttonSelectedName = str));
				ModifyColor("Disabled Color", new Ref<string>(() => currentHandler.buttonDisabledName, str => currentHandler.buttonDisabledName = str));
			}
			else
			{
				EditorGUILayout.HelpBox("What the fuck is going on", MessageType.Error);
			}
		}

		public void ModifyColor(string label, Ref<string> colorName)
		{
			// Display warning message if its not present in all available formats
			List<string> unsupportedFormats = new();
			IReadOnlyList<ColorFormat> available = ColorFormat.GetAllFormats().developerFormats;
			for (int i = 0; i < available.Count; i++)
				if (!available[i].TryGetColor(colorName, out _))
					unsupportedFormats.Add(available[i].FormatName);
			if (unsupportedFormats.Count > 0)
				EditorGUILayout.HelpBox($"There are some available color themes that does not support the assigned color: {string.Join(", ", unsupportedFormats)}", MessageType.Warning, true);
			// Starting here normally
			List<string> colorNames = GetAllColorNames();
			// Popup for changing name, taking from default
			int currentIndex = colorNames.IndexOf(colorName);
			if (currentIndex == -1)
			{
				colorName.Value = defaultValues[0];
				currentIndex = 0;
			}
			var menu = new GenericMenu();
			for (int i = 0; i < defaultValues.Count; i++)
			{
				int delegateIndex = i;
				menu.AddItem(new GUIContent($"{defaultValues[i]}"), i == currentIndex, () => SetDirty(delegateIndex));
			}
			menu.AddSeparator("");
			for (int i = defaultValues.Count; i < colorNames.Count; i++)
			{
				int delegateIndex = i;
				menu.AddItem(new GUIContent($"{colorNames[i]}"), i == currentIndex, () => SetDirty(delegateIndex));
			}
			Rect fullRect = GUILayoutUtility.GetRect(Screen.width, 20f);
			Rect textRect = fullRect;
			textRect.width *= 0.4f;
			Rect popupRect = fullRect;
			popupRect.xMin = textRect.xMax + 2;
			EditorGUI.LabelField(textRect, label);
			if (GUI.Button(popupRect, new GUIContent(colorNames[currentIndex]), EditorStyles.popup))
				menu.ShowAsContext();

			void SetDirty(int setValue)
			{
				colorName.Value = colorNames[setValue];
				if (Application.isPlaying)
					currentHandler.UpdateColors();
				else
					DirtyAuto.SetDirty(currentHandler);
			}
		}

		private static void TestSwitch()
		{
			StringBuilder modified = new("Modified:\n");
			string[] guids = AssetDatabase.FindAssets("t:Prefab");
			for (int i = 0; i < guids.Length; i++)
			{
				string path = AssetDatabase.GUIDToAssetPath(guids[i]);
				GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

				if (RecursiveCheck(prefab.transform))
					PrefabUtility.SaveAsPrefabAsset(prefab, path);

				bool RecursiveCheck(Transform targetTransform)
				{
					// Do recursive checks first
					bool falseOut = false;
					for (int i = 0; i < targetTransform.childCount; i++)
						falseOut |= RecursiveCheck(targetTransform.GetChild(i));

					if (!targetTransform.TryGetComponent(out UIThemeHandler handler))
						return falseOut;
					Component old = handler.TargetComponent;
					if (old == null && targetTransform.TryGetComponent(out Graphic graphic))
					{
						handler.TargetComponent = graphic;
						modified.AppendLine($"\t{prefab.name}");
						return true;
					}
					return falseOut;
				}
			}
			Debug.Log(modified.ToString());
		}
	}
}
#endif