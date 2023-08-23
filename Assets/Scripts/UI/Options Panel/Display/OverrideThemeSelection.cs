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
		public override int InitialValue
		{
			get
			{
				int index = Values.ToList().FindIndex(format => format.FormatName == ColorFormat.ActiveFormat.FormatName);
				return index == -1 ? 0 : index;
			}
		}

		protected override void Awake()
		{
			base.Awake();
		}

		public override List<KeyValuePair<string, ColorFormat>> DefinedPairs
		{
			get
			{
				IReadOnlyList<ColorFormat> list = ColorFormat.GetPlayableFormats();
				List<KeyValuePair<string, ColorFormat>> pairs = new(list.Count);
				for (int i = 0; i < list.Count; i++)
					pairs.Add(new KeyValuePair<string, ColorFormat>(list[i].FormatName, list[i]));
				return pairs;
			}
		}


		protected override void PickedChoice(int index)
		{
			base.PickedChoice(index);
			ColorFormat currentFormat = Pairs[index].Value;
			ColorFormat.SetFormat(currentFormat, true);
		}
	}
}