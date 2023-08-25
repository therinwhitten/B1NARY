namespace B1NARY.UI
{
	using UnityEngine;
	using System;
	using TMPro;
	using System.Collections.Generic;
	using System.Linq;
	using OVSXmlSerializer;
	using B1NARY.UI.Colors;
	using B1NARY.UI.Globalization;

	[RequireComponent(typeof(DropdownGlobalizer))]
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
				ColorFormat[] dictionary = ColorFormat.GetPlayableFormats().ToArray();
				List<KeyValuePair<string, ColorFormat>> pairs = new(dictionary.Length);
				string[] languageKeys = GetComponent<DropdownGlobalizer>()[Languages.Instance[0]];
				Debug.Log(string.Join('|', languageKeys), this);
				for (int i = 0; i < languageKeys.Length; i++)
				{
					string key = languageKeys[i];
					ColorFormat format = Array.Find(dictionary, format => format.FormatName == key);
					if (format != null)
						pairs.Add(new KeyValuePair<string, ColorFormat>(key, format));
					else
						Debug.LogWarning($"{key} format doesn't exist in player formats!");
				}
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