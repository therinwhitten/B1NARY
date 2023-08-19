namespace B1NARY.UI.Colors
{
	using System;
	using UnityEngine;
	using TMPro;
	using UnityEngine.UI;
	using UI;
	using System.Text;
	using System.Linq;
	using System.Reflection;
	using B1NARY.DesignPatterns;
	using System.Collections.Generic;
	using UnityEngine.Rendering;
	using B1NARY.Scripting;

	/// <summary>
	/// Allows you to easily change a single gameObject with the <see cref="Image"/>
	/// component to a color. By enabling, you change it via the parameters the
	/// component stores. Disabling reverts to it's previous known color.
	/// </summary>
	/// <seealso cref="MonoBehaviour"/>
	[RequireComponent(typeof(Graphic))]
	public class UIThemeHandler : MonoBehaviour
	{
		public static Color GetColor(string name)
		{
			if (ColorFormat.CurrentFormat.TryGetColor(name, out Color color))
				return color;
			throw new NullReferenceException($"'{name}' is not located within the currently " +
				$"equipped format: {ColorFormat.CurrentFormat.FormatName}.");
		}

		public string imageThemeName = ColorFormat.COLOR_NAME_SECONDARY,
			buttonHighlightedName = ColorFormat.COLOR_NAME_PRIMARY,
			buttonPressedName = ColorFormat.COLOR_NAME_PRIMARY,
			buttonSelectedName = ColorFormat.COLOR_NAME_PRIMARY,
			buttonDisabledName = ColorFormat.COLOR_NAME_PRIMARY;

		/// <summary>
		/// Contains the first <see cref="UnityEngine.Color"/> or <see cref="ColorBlock"/>,
		/// these are always value types, so these are referenced via Func.
		/// </summary>
		public Ref<object> ColorEdit 
		{
			get
			{
				if (m_colorEdit == null || m_colorEdit.Value == null)
				{
					m_currentTarget = null;
					_ = CurrentTarget;
				}

				if (m_colorEdit == null)
					throw new NullReferenceException($"despite {nameof(CurrentTarget)} being called, there is no reference for the color!");
				return m_colorEdit;
			} 
			private set => m_colorEdit = value; 
		}
		private Ref<object> m_colorEdit;

		public Component CurrentTarget 
		{ 
			get 
			{
				if (m_currentTarget != null)
					return m_currentTarget;
				Component[] components = GetComponents<Component>();
				for (int i = 0; i < components.Length; i++)
				{
					Component currentComponent = components[i];
					Type componentType = currentComponent.GetType();
					PropertyInfo[] properties = componentType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
					// Get any color properties in the component that has any color parameter.
					for (int ii = 0; ii < properties.Length; ii++)
					{
						PropertyInfo currentProperty = properties[ii];
						if (properties[ii].PropertyType == typeof(Color))
						{
							m_colorEdit = new Ref<object>(() => currentProperty.GetValue(currentComponent), set => currentProperty.SetValue(currentComponent, set));
							m_currentTarget = currentComponent;
							return m_currentTarget;
						}
						if (properties[ii].PropertyType == typeof(ColorBlock))
						{
							m_colorEdit = new Ref<object>(() => currentProperty.GetValue(currentComponent), set => currentProperty.SetValue(currentComponent, set));
							m_currentTarget = currentComponent;
							return m_currentTarget;
						}
					}
					// Get any color block properties in the component that has any color block parameter.
				}
				throw new MissingComponentException("There is no components " +
					$"to hold onto that has a {nameof(UnityEngine.Color)} or {nameof(ColorBlock)}");
			}
		}
		private Component m_currentTarget;


		protected virtual void Start()
		{
			UpdateColors();
		}
		protected virtual void OnEnable()
		{
			ColorFormat.ChangedFormat += UpdateColors;
		}
		protected virtual void OnDisable()
		{
			ColorFormat.ChangedFormat -= UpdateColors;
		}
		public void UpdateColors(ColorFormat format = null)
		{
			if (ColorEdit.Value is Color)
				ColorEdit.Value = GetColor(imageThemeName);
			else if (ColorEdit.Value is ColorBlock block)
				ColorEdit.Value = new ColorBlock()
				{
					colorMultiplier = block.colorMultiplier,
					disabledColor = GetColor(buttonDisabledName),
					fadeDuration = block.fadeDuration,
					highlightedColor = GetColor(buttonHighlightedName),
					normalColor = GetColor(imageThemeName),
					pressedColor = GetColor(buttonPressedName),
					selectedColor = GetColor(buttonSelectedName),
				};
			else
				throw new IndexOutOfRangeException(CurrentTarget.ToString());
		}
	}
}
#if UNITY_EDITOR
namespace B1NARY.UI.Colors.Editor
{
	using B1NARY.Editor;
	using DG.DemiEditor;
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.IO;
	using System.Linq;
	using System.Reflection;
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
			List<ColorFormat.FormatFile> allFormats = ColorFormat.AllFormats;
			for (int i = 0; i < allFormats.Count; i++)
			{
				string[] allKeys = new string[allFormats[i].Format.ExtraUIColors.Count];
				allFormats[i].Format.ExtraUIColors.Keys.CopyTo(allKeys, 0);
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
			if (!Application.isPlaying)
			{
				int count = 0;
				Transform transform = currentHandler.transform;
				for (int i = 0; i < transform.childCount; i++)
				{
					if (transform.GetChild(i).GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
						.Any(info => info.PropertyType == typeof(Color) || info.PropertyType == typeof(ColorBlock)))
						count++;
				}
				if (count > 1)
					EditorGUILayout.HelpBox("There is more than one " +
						"color-related blocks detected in the components, " +
						"make sure that the one you want to modify is at the top!",
						MessageType.Info);
				EditorGUILayout.Space();
			}

			try
			{
				string componentName = currentHandler.CurrentTarget.GetType().ToString();
				EditorGUILayout.LabelField($"Current Configuration: {componentName.Substring(componentName.LastIndexOf('.') + 1).Replace('_', ' ')}", EditorStyles.boldLabel);
			}
			catch (MissingComponentException ex)
			{
				EditorGUILayout.HelpBox(ex.Message, MessageType.Error);
				throw;
			}

			if (currentHandler.ColorEdit.Value is Color)
			{
				ModifyColor("Color Option", new Ref<string>(() => currentHandler.imageThemeName, str => currentHandler.imageThemeName = str));
			}
			else if (currentHandler.ColorEdit.Value is ColorBlock)
			{
				ModifyColor("Normal Color", new Ref<string>(() => currentHandler.imageThemeName, str => currentHandler.imageThemeName = str));
				ModifyColor("Highlighted Color", new Ref<string>(() => currentHandler.buttonHighlightedName, str => currentHandler.buttonHighlightedName = str));
				ModifyColor("Pressed Color", new Ref<string>(() => currentHandler.buttonPressedName, str => currentHandler.buttonPressedName = str));
				ModifyColor("Selected Color", new Ref<string>(() => currentHandler.buttonSelectedName, str => currentHandler.buttonSelectedName = str));
				ModifyColor("Disabled Color", new Ref<string>(() => currentHandler.buttonDisabledName, str => currentHandler.buttonDisabledName = str));
			}
			else
			{
				throw new IndexOutOfRangeException(currentHandler.CurrentTarget.ToString());
			}
		}

		public void ModifyColor(string label, Ref<string> colorName)
		{
			// Display warning message if its not present in all available formats
			List<string> unsupportedFormats = new();
			for (int i = 0; i < ColorFormat.AvailableFormats.Count; i++)
				if (!ColorFormat.AvailableFormats[i].Format.TryGetColor(colorName, out _))
					unsupportedFormats.Add(ColorFormat.AvailableFormats[i].Format.FormatName);
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
	}
}
#endif