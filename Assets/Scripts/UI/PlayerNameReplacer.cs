namespace B1NARY.UI
{
	using B1NARY.DataPersistence;
	using System;
	using System.Collections.Generic;
	using TMPro;

	public sealed class PlayerNameReplacer : CachedMonobehaviour
	{
		private void OnEnable()
		{
			TMP_Text text = GetComponent<TMP_Text>();
			text.text = text.text.Replace("MC", SaveSlot.Instance.scriptDocumentInterface.PlayerName);
			SaveSlot.Instance.scriptDocumentInterface.strings.UpdatedValue += UpdateName;
		}
		private void UpdateName(string key, string oldValue, string newValue, ScriptDocumentInterface.Collection<string> source)
		{
			if (ScriptDocumentInterface.playerNameKey != key)
				return;
			TMP_Text text = GetComponent<TMP_Text>();
			text.text = text.text.Replace(oldValue, newValue);
		}
		private void OnDisable()
		{
			SaveSlot.Instance.scriptDocumentInterface.strings.UpdatedValue -= UpdateName;
		}
	}
}