namespace B1NARY
{
	using UnityEngine;

	public static class PlayerPrefsShortcuts
	{
		public static bool IsHentaiEnabled { get => PlayerPrefs.GetInt(hentaiKey, 0) == 1; set => PlayerPrefs.SetInt(hentaiKey, value ? 1 : 0); }
		private const string hentaiKey = "B1NARYHentaiModeEnabled";

		
	}
}
