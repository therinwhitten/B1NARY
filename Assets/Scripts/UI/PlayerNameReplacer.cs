namespace B1NARY.UI
{
	using B1NARY.CharacterManagement;
	using B1NARY.DataPersistence;
	using System;
	using System.Collections.Generic;
	using TMPro;

	public sealed class PlayerNameReplacer : CachedMonobehaviour
	{
		public bool showOnlyPlayer = true;

		public static string ReplaceText(in string text)
		{
			if (text == SaveSlot.DEFAULT_NAME) 
				return SaveSlot.ActiveSlot.PlayerName;
			if (string.IsNullOrEmpty(text))
				return "Indescribable Horror";
			return text;
		}
		public bool ShouldChangeName(Character? character)
		{
			if (!character.HasValue)
				return false;
			if (!showOnlyPlayer)
				return true;
			if (CharacterManager.Instance.ActiveCharacter.Value.controller.CharacterName == "MC")
				return true;
			return false;
		}
		private void OnEnable()
		{
			TMP_Text text = GetComponent<TMP_Text>();
			CharacterManager.Instance.ActiveCharacterChanged += (character) =>
			{
				if (ShouldChangeName(character))
					text.text = ReplaceText(character.Value.controller.CharacterName);
			};
			SaveSlot.ActiveSlot.strings.UpdatedValue += UpdateName;
			text.text = ReplaceText(text.text);
		}
		private void UpdateName(string key, string oldValue, string newValue, Collection<string> source)
		{
			if (key != SaveSlot.KEY_PLAYER_NAME)
				return;
			TMP_Text text = GetComponent<TMP_Text>();
			text.text = ReplaceText(text.text);
		}
		private void OnDisable()
		{
			SaveSlot.ActiveSlot.strings.UpdatedValue -= UpdateName;
		}
	}
}