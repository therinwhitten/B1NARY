namespace B1NARY.UI
{
	using UnityEngine;
	using System;
	using TMPro;
	using static UIThemeHandler;
	using System.Collections.Generic;
	using System.Linq;
	using OVSXmlSerializer;

	public class OverrideThemeSelection : DropdownPanel<ColorFormat>
	{
		public override int InitialValue => Values.ToList().IndexOf(ColorFormat.CurrentTheme);
		
		protected override void Awake()
		{
			base.Awake();
		}

		public override List<KeyValuePair<string, ColorFormat>> DefinedPairs =>
			ColorFormat.PlayerFormats
			.Select(format => new KeyValuePair<string, ColorFormat>(format.FormatName, format)).ToList();
		protected override void PickedChoice(int index)
		{
			base.PickedChoice(index);
			ColorFormat currentFormat = Pairs[index].Value;
			if (ReferenceEquals(ColorFormat.DefaultTheme, currentFormat))
			{
				ColorFormat.RemoveOverride();
				return;
			}
			ColorFormat.Override(currentFormat);
		}
	}
}