namespace B1NARY.UI
{
	using System;
	using System.Collections.Generic;
	using TMPro;

	public sealed class HentaiEnabledDropdown : DropdownPanel<bool>
	{
		private static int HentaiEnabledToInt => Convert.ToInt32(PlayerConfig.Instance.HentaiEnabled);
		public override int InitialValue => HentaiEnabledToInt;
		public override List<KeyValuePair<string, bool>> DefinedPairs => new List<KeyValuePair<string, bool>>()
		{
			new KeyValuePair<string, bool>("Twitch", false),
			new KeyValuePair<string, bool>("Hentai", true),
		};
		protected override void Awake()
		{
			base.Awake();
			ChangedValue += (boolean, index) => PlayerConfig.Instance.HentaiEnabled = boolean; 
			PlayerConfig.Instance.Serialize();
		}
	}
}
#if UNITY_EDITOR

#endif