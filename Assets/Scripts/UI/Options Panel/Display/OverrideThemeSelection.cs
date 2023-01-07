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
		public override int InitialValue => Values.ToList().IndexOf(ColorFormat.CurrentFormat.name);
		
		protected override void Awake()
		{
			base.Awake();
		}

		public override List<KeyValuePair<string, string>> DefinedPairs =>
			Resources.LoadAll<ColorFormat>(ColorFormat.resourcesColorThemePath)
			.Select(format => new KeyValuePair<string, string>(format.name, format.name)).ToList();
		protected override void PickedChoice(int index)
		{
			if (ColorFormat.defaultKey == Pairs[index].Value)
			{
				ColorFormat.HasOverridedTheme = false;
				return;
			}
			ColorFormat.OverridedTheme = Pairs[index].Value;
		}
	}
}