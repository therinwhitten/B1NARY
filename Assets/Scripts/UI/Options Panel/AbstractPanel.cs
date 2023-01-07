namespace B1NARY.UI
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using TMPro;
	using UnityEngine;

	[RequireComponent(typeof(TMP_Dropdown))]
	public abstract class DropdownPanel<T> : MonoBehaviour
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
		/// <see cref="Values"/>
		/// </summary>
		public T CurrentValue => Values[CurrentSelection];
		/// <summary>
		/// When the value changes; Equivalent to <see cref="TMP_Dropdown.onValueChanged"/>
		/// </summary>
		public event Action<T, int> ChangedValue;
		/// <summary>
		/// What is the value when it first starts up, so it wont start with a 0?
		/// </summary>
		public abstract int InitialValue { get; }

		[HideInInspector]
		public TMP_Dropdown dropdown;
		/// <summary>
		/// What is shown directly to the dropdown. Keep in mind, this should have
		/// the same count as <see cref="Values"/>!
		/// </summary>
		public virtual List<string> Visuals => Values.Select(val => val.ToString()).ToList();
		/// <summary>
		/// When the object is first initialized, and doesn't have any data to
		/// start with.
		/// </summary>
		public virtual List<T> DefinedValues => new List<T>();
		/// <summary>
		/// The Currently stored values.
		/// </summary>
		public List<T> Values
		{ 
			get
			{
				if (m_values is null)
					return new List<T>();
				return m_values;
			}
			set => m_values = value;
		}
		protected List<T> m_values;

		protected virtual void Awake()
		{
			if (m_values is null)
				m_values = DefinedValues;
			if (Values.Count != Visuals.Count)
				Debug.LogError("The length of the values does not match the visuals array!");
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
			dropdown.AddOptions(Visuals);
		}

		/// <summary>
		/// What to do when the dropdown changes value.
		/// </summary>
		/// <param name="index"></param>
		protected virtual void PickedChoice(int index)
		{
			CurrentSelection = index;
			ChangedValue.Invoke(CurrentValue, index);
		}
	}
}