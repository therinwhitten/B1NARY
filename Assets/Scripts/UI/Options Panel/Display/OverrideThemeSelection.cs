namespace B1NARY.UI
{
	using UnityEngine;
	using System;
	using TMPro;
	using System.Collections.Generic;
	using System.Linq;
	using OVSXmlSerializer;
	using B1NARY.UI.Colors;

	public class OverrideThemeSelection : DropdownPanel<ColorFormat>
	{
		public override int InitialValue => Values.ToList().FindIndex(format => format.FormatName == ColorFormat.ActiveFormat.FormatName);
		
		protected override void Awake()
		{
			base.Awake();
		}

		public override List<KeyValuePair<string, ColorFormat>> DefinedPairs =>
			ColorFormat.GetPlayerFormats()
			.Select(format => new KeyValuePair<string, ColorFormat>(format.FormatName, format)).ToList();
		protected override void PickedChoice(int index)
		{
			base.PickedChoice(index);
			ColorFormat currentFormat = Pairs[index].Value;
			ColorFormat.SetFormat(currentFormat, true);
		}
	}
}