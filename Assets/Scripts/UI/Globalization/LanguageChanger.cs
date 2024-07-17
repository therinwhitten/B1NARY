namespace B1NARY.UI.Globalization
{
	using B1NARY.DataPersistence;
	using System;
	using UnityEngine;

	public class LanguageChanger : MonoBehaviour
	{
		public void ChangeLanguageTo(string language)
		{
			if (Languages.Instance.Contains(language) == false)
				throw new InvalidCastException(language);
			PlayerConfig.Instance.language.Value = language;
		}
		public void ChangeLanguageTo(int languageIndex)
		{
			PlayerConfig.Instance.language.Value = Languages.Instance[languageIndex];
		}
	}
}
#if UNITY_EDITOR
namespace B1NARY.UI.Globalization.Editor
{
	using System;
	using UnityEditor;

	[CustomEditor(typeof(LanguageChanger))]
	public class LanguageChangerEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			Languages.Instance.Editor_OnGUI();
		}
	}
}
#endif