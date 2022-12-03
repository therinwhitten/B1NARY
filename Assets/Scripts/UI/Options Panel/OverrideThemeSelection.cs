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

		public override int InitialValue => Values.IndexOf(defaultFormat);

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
			base.DefineDropdown();
			dropdown.value = Visuals.IndexOf(defaultFormat.ToString());
		}

		protected override void PickedChoice(int index)
		{
			Overrided = index != 0;
			defaultFormat = Values[index];
		}
	}
}