namespace B1NARY.UI
{
	using UnityEngine;
	using System;
	using TMPro;
	using static UIThemeHandler;
	using System.Collections.Generic;

	public class OverrideThemeSelection : DropdownPanel<ColorFormat>
	{
		const string key = "OverridedTheme";
		public static bool Overrided
		{
			get => PlayerPrefs.GetInt(key, 0) == 1; set
			{
				PlayerPrefs.SetInt(key, value ? 1 : 0);
				PlayerPrefs.Save();
			}
		}

		public ColorFormat defaultFormat;
		protected override void Awake()
		{
			base.Awake();
			if (defaultFormat == null)
				defaultFormat = CurrentlyEquippedFormat;
			else
			{
				if (defaultFormat != CurrentlyEquippedFormat)
					CurrentlyEquippedFormat = defaultFormat;
			}

		}

		protected override void DefineDropdown()
		{
			dropdown = GetComponent<TMP_Dropdown>();
			dropdown.ClearOptions();
			var options = new List<TMP_Dropdown.OptionData>(values.Length + 1) {
				new TMP_Dropdown.OptionData(defaultFormat.name)
			};
			for (int i = 0; i < values.Length; i++)
				options.Add(new TMP_Dropdown.OptionData(values[i].name));
			dropdown.AddOptions(options);
		}

		public override void PickedChoice(int index)
		{
			Overrided = index != 0;
			defaultFormat = values[index];
		}
	}
}