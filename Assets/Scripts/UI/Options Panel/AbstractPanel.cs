namespace B1NARY.UI
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using TMPro;
	using UnityEngine;

	[RequireComponent(typeof(TMP_Dropdown))]
	public abstract class DropdownPanel<TValue> : MonoBehaviour
	{
		/// <summary>
		/// The <see cref="dropdown"/>'s value.
		/// </summary>
		public int CurrentSelection
		{
			get => dropdown.value;
			set => dropdown.value = value;
		}
		/// <summary>
		/// Uses <see cref="CurrentSelection"/> to get the current value of 
		/// <see cref="Pairs"/>
		/// </summary>
		public TValue CurrentValue => Pairs[CurrentSelection].Value;
		/// <summary>
		/// When the value changes; Equivalent to <see cref="TMP_Dropdown.onValueChanged"/>
		/// </summary>
		public event Action<TValue, int> ChangedValue;
		/// <summary>
		/// What is the value when it first starts up, so it wont start with a 0?
		/// </summary>
		public abstract int InitialValue { get; }

		[HideInInspector]
		public TMP_Dropdown dropdown;
		///// <summary>
		///// What is shown directly to the dropdown. Keep in mind, this should have
		///// the same count as <see cref="Pairs"/>!
		///// </summary>
		//public virtual List<string> Visuals => Pairs.Select(val => val.ToString()).ToList();
		/// <summary>
		/// When the object is first initialized, and doesn't have any data to
		/// start with.
		/// </summary>
		public virtual List<KeyValuePair<string, TValue>> DefinedPairs => new List<KeyValuePair<string, TValue>>();
		/// <summary>
		/// The Currently stored values.
		/// </summary>
		public List<KeyValuePair<string, TValue>> Pairs
		{
			get => m_pairs;
			set => m_pairs = value;
		}
		protected List<KeyValuePair<string, TValue>> m_pairs;
		public IEnumerable<string> Keys => Pairs.Select(pair => pair.Key);
		public IEnumerable<TValue> Values => Pairs.Select(pair => pair.Value);

		protected virtual void Awake()
		{
			m_pairs = DefinedPairs;
			DefineDropdown();
			dropdown.value = InitialValue;
			dropdown.onValueChanged.AddListener(PickedChoice);
		}

		/// <summary>
		/// Defines all the values from <see cref="Visuals"/> into the dropdown.
		/// </summary>
		protected virtual void DefineDropdown()
		{
			dropdown = GetComponent<TMP_Dropdown>();
			dropdown.ClearOptions();
			dropdown.AddOptions(Pairs.Select(pair => pair.Key).ToList());
		}

		/// <summary>
		/// What to do when the dropdown changes value.
		/// </summary>
		/// <param name="index"></param>
		protected virtual void PickedChoice(int index)
		{
			CurrentSelection = index;
			ChangedValue?.Invoke(CurrentValue, index);
		}
	}
}
#if UNITY_EDITOR
namespace B1NARY.UI.Editor
{
	using UnityEditor;
	using UnityEngine;

	public abstract class DropDownEditor<T> : Editor
	{
		private DropdownPanel<T> panel;
		public override void OnInspectorGUI()
		{
			panel = (DropdownPanel<T>)target;
			for (int i = 0; i < panel.Pairs.Count; i++)
			{
				Rect fullRect = GUILayoutUtility.GetRect(Screen.width, 20f);
				EditorGUI.LabelField(new Rect(fullRect) { width = (fullRect.width / 2f) - 1f }, panel.Pairs[i].Key);
				EditorGUI.LabelField(new Rect(fullRect) { xMin = (fullRect.width / 2f) + 1f }, panel.Pairs[i].Value.ToString());
			}
		}
	}
}
#endif