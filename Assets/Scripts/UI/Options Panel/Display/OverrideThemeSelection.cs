namespace B1NARY.UI
{
	using UnityEngine;
	using System;
	using TMPro;
	using static UIThemeHandler;
	using System.Collections.Generic;
	using System.Linq;

	public class OverrideThemeSelection : DropdownPanel<string>
	{
		public override int InitialValue => Values.IndexOf(ColorFormat.CurrentFormat.name);
		
		protected override void Awake()
		{
			base.Awake();
		}

		public override List<string> DefinedValues =>
			Resources.LoadAll<ColorFormat>(ColorFormat.resourcesColorThemePath)
			.Select(format => format.name).ToList();
		protected override void PickedChoice(int index)
		{
			if (ColorFormat.defaultKey == Values[index])
			{
				ColorFormat.HasOverridedTheme = false;
				return;
			}
			ColorFormat.OverridedTheme = Values[index];
		}
	}
}