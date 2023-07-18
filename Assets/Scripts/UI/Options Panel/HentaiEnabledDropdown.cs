namespace B1NARY.UI
{
	using B1NARY.DataPersistence;
	using System;
	using System.Collections.Generic;
	using TMPro;

	public sealed class HentaiEnabledDropdown : DropdownPanel<bool>
	{
		private static int HentaiEnabledToInt => Convert.ToInt32(PlayerConfig.Instance.hEnable.Value);
		public override int InitialValue => HentaiEnabledToInt;
		public override List<KeyValuePair<string, bool>> DefinedPairs => new List<KeyValuePair<string, bool>>()
		{
			new KeyValuePair<string, bool>("Twitch", false),
			new KeyValuePair<string, bool>("Hentai", true),
		};
		protected override void Awake()
		{
			base.Awake();
			ChangedValue += (boolean, index) => PlayerConfig.Instance.hEnable.Value = boolean;
		}
	}
}
#if UNITY_EDITOR

#endif