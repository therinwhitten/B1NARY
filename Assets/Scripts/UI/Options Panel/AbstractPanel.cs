namespace B1NARY.UI
{
	using System;
	using System.Collections.Generic;
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

		[HideInInspector]
		public TMP_Dropdown dropdown;
		public T[] values;

		protected virtual void Awake()
		{
			DefineDropdown();
			dropdown.onValueChanged.AddListener(PickedChoice);
		}

		protected virtual void DefineDropdown()
		{
			dropdown = GetComponent<TMP_Dropdown>();
			dropdown.ClearOptions();
			var options = new List<TMP_Dropdown.OptionData>(values.Length);
			for (int i = 0; i < values.Length; i++)
				options.Add(new TMP_Dropdown.OptionData(values[i].ToString()));
			dropdown.AddOptions(options);
		}

		public abstract void PickedChoice(int index);
	}
}