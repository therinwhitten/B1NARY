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
			PlayerConfig.Instance.language.ValueChanged += UpdatedLanguage;
		}
		private void OnDestroy()
		{
			PlayerConfig.Instance.language.ValueChanged -= UpdatedLanguage;
		}
		public override int InitialValue => Values.ToList().IndexOf(PlayerConfig.Instance.language);

		private void UpdatedLanguage(string newLanguage)
		{
			int index = Pairs.FindIndex(pair => pair.Key.Contains(newLanguage));
			PickedChoice(index);
		}
		protected override void PickedChoice(int index)
		{
			PlayerConfig.Instance.language.ValueChanged -= UpdatedLanguage;
			base.PickedChoice(index);
			PlayerConfig.Instance.language.Value = CurrentValue;
			PlayerConfig.Instance.language.ValueChanged += UpdatedLanguage;
		}
	}
}