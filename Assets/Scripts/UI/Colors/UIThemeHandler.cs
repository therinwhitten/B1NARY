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

#warning This seems a bit inefficient. Feel free to optimize this one!
	/// <summary>
	/// Allows you to easily change a single gameObject with the <see cref="Image"/>
	/// component to a color. By enabling, you change it via the parameters the
	/// component stores. Disabling reverts to it's previous known color.
	/// </summary>
	/// <seealso cref="MonoBehaviour"/>
	[RequireComponent(typeof(Graphic))]
	public class UIThemeHandler : MonoBehaviour
	{
		public enum Option
		{
			Primary,
			Secondary,
			Custom
		}

		public static Color GetColor(Option option)
		{
			switch (option)
			{
				case Option.Primary:
					return ColorFormat.CurrentFormat.primaryUI;
				case Option.Secondary:
					return ColorFormat.CurrentFormat.SecondaryUI;
				case Option.Custom:
					throw new ArgumentException($"although {option} is a enum, you need" +
						$"another argument or name to differentiate other settings!");
				default:
					throw new IndexOutOfRangeException(option.ToString());
			}
		}
		public static Color GetColor(string name)
		{
			if (Enum.TryParse(name, out Option option))
				return GetColor(option);
			if (ColorFormat.CurrentFormat.ExtraUIColors.TryGetValue(name, out Color color))
				return color;
			throw new NullReferenceException($"'{name}' is not located within the currently " +
				$"equipped format: {ColorFormat.CurrentFormat.FormatName}.");
		}

#warning TODO: make this ctor thing more efficient!
		public string imageThemeName = Option.Secondary.ToString(),
			buttonHighlightedName = Option.Primary.ToString(),
			buttonPressedName = Option.Primary.ToString(),
			buttonSelectedName = Option.Primary.ToString(),
			buttonDisabledName = Option.Primary.ToString();
		public Color Color => GetColor(imageThemeName);

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
	using B1NARY.UI;
	using System;
	using System.Collections.ObjectModel;
	using System.Linq;
	using System.Reflection;
	using UnityEditor;
	using UnityEngine;
	using UnityEngine.UI;
	using Object = UnityEngine.Object;

	[CustomEditor(typeof(UIThemeHandler))]
	public class UIThemeHandlerEditor : Editor
	{
		internal static UIThemeHandler.Option[] options = (UIThemeHandler.Option[])Enum.GetValues(typeof(UIThemeHandler.Option));
		internal static string[] optionNames = Enum.GetNames(typeof(UIThemeHandler.Option));



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
				//EditorGUILayout.ObjectField(new GUIContent("Color Format [readonly]", "The currently used format across all Theme Handlers"), , typeof(ColorFormat), false);
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
			bool hasChanges = false;
			if (currentHandler.ColorEdit.Value is Color)
				hasChanges = ModifyColor("Color Option", new Ref<string>(() => currentHandler.imageThemeName, str => currentHandler.imageThemeName = str));
			else if (currentHandler.ColorEdit.Value is ColorBlock)
				hasChanges = ModifyColor("Normal Color", new Ref<string>(() => currentHandler.imageThemeName, str => currentHandler.imageThemeName = str))
				 | ModifyColor("Highlighted Color", new Ref<string>(() => currentHandler.buttonHighlightedName, str => currentHandler.buttonHighlightedName = str))
				 | ModifyColor("Pressed Color", new Ref<string>(() => currentHandler.buttonPressedName, str => currentHandler.buttonPressedName = str))
				 | ModifyColor("Selected Color", new Ref<string>(() => currentHandler.buttonSelectedName, str => currentHandler.buttonSelectedName = str))
				 | ModifyColor("Disabled Color", new Ref<string>(() => currentHandler.buttonDisabledName, str => currentHandler.buttonDisabledName = str));
			else
				throw new IndexOutOfRangeException(currentHandler.CurrentTarget.ToString());
			if (hasChanges)
			{
				if (Application.isPlaying)
					currentHandler.UpdateColors();
				else
					EditorUtility.SetDirty(currentHandler);
			}
		}

		public bool ModifyColor(string label, Ref<string> colorName)
		{
			string old = colorName;
			// Popup box for defaults
			int currentIndex = Array.IndexOf(optionNames, colorName);
			if (currentIndex == -1)
				currentIndex = Array.IndexOf(options, UIThemeHandler.Option.Custom);
			int newIndex = EditorGUILayout.Popup(label, currentIndex, optionNames);
			if (newIndex != currentIndex)
				colorName.Value = optionNames[newIndex];
			if (Array.IndexOf(options, UIThemeHandler.Option.Custom) == currentIndex)
			{
				CustomMenu(colorName);
			}
			return colorName != old;
		}

		public void CustomMenu(Ref<string> newValueAction)
		{
			Rect fullRect = EditorGUI.IndentedRect(GUILayoutUtility.GetRect(Screen.width, 20f)),
						popupRect = new Rect(fullRect) { width = 20f };
			fullRect.xMin += 22f;
			newValueAction.Value = EditorGUI.TextField(fullRect, newValueAction);

			if (!GUI.Button(popupRect, "", EditorStyles.foldout))
				return;
			// Context Menu
			//var menu = new GenericMenu();
			////menu.AddDisabledItem(new GUIContent($"Current Selection: {ColorFormat..name}"));
			//menu.AddSeparator("");
			//for (int i = 0; i < UIThemeHandler.CurrentlyEquippedFormat.SavedPairs.Amount; i++)
			//{
			//	int currentIterative = i;
			//	string capturedKey = UIThemeHandler.CurrentlyEquippedFormat.SavedPairs[i].Key;
			//	menu.AddItem(new GUIContent(capturedKey), newValueAction.Value == capturedKey, () =>
			//	{
			//		newValueAction.Value = capturedKey;
			//		EditorUtility.SetDirty(target);
			//	});
			//}
			//menu.ShowAsContext();
		}
	}
}
#endif