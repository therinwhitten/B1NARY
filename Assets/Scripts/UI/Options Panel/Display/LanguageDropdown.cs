namespace B1NARY.UI
{
	using System.Linq;
	using System.Collections.Generic;
	using UnityEngine;
	using B1NARY.UI.Globalization;
	using B1NARY.DataPersistence;

	public sealed class LanguageDropdown : DropdownPanel<string>
	{
		public override List<KeyValuePair<string, string>> DefinedPairs
		{
			get
			{
				return Languages.Instance
					.Select(language => new KeyValuePair<string, string>(language, language))
					.ToList();
			}
		}

		protected override void Awake()
		{
			base.Awake();
		}
		public override int InitialValue => Values.ToList().IndexOf(PlayerConfig.Instance.language);

		protected override void PickedChoice(int index)
		{
			base.PickedChoice(index);
			PlayerConfig.Instance.language.Value = CurrentValue;
		}
	}
}