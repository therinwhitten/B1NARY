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
		public int CurrentSelection
		{
			get => dropdown.value;
			set => dropdown.value = value;
		}
		public T CurrentValue => values[CurrentSelection];
		public event Action<int> ChangedValue = integer => { };

		[HideInInspector]
		public TMP_Dropdown dropdown;
		public virtual List<string> Visuals => values.Select(val => val.ToString()).ToList();
		public virtual List<T> DefinedValues { get; } = null;
		public List<T> values;

		protected virtual void Awake()
		{
			if (DefinedValues != null && values == null)
				values = DefinedValues;
			if (values.Count != Visuals.Count)
				Debug.LogError("The length of the values does nto match the visuals array!");
			DefineDropdown();
			dropdown.onValueChanged.AddListener(PickedChoice);
			dropdown.onValueChanged.AddListener(ChangedValue.Invoke);
		}

		protected virtual void DefineDropdown()
		{
			dropdown = GetComponent<TMP_Dropdown>();
			dropdown.ClearOptions();
			dropdown.AddOptions(Visuals);
		}

		public abstract void PickedChoice(int index);
	}
}