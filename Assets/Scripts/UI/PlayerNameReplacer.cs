namespace B1NARY.UI
{
	using B1NARY.CharacterManagement;
	using B1NARY.DataPersistence;
	using System;
	using System.Collections.Generic;
	using TMPro;

	public sealed class PlayerNameReplacer : CachedMonobehaviour
	{
		public static string ReplaceText(in string text)
		{
			if (text == SaveSlot.DEFAULT_NAME)
				return SaveSlot.ActiveSlot.PlayerName;
			if (string.IsNullOrWhiteSpace(text))
				return "Indescribable Horror";
			return text;
		}

		public TMP_Text text;
		public bool displayOnlyPlayerName = false;

		private void Reset()
		{
			text = GetComponent<TMP_Text>();
		}
		private void OnEnable()
		{
			if (SaveSlot.ActiveSlot == null)
			{
				gameObject.SetActive(false);
				return;
			}
			SaveSlot.ActiveSlot.strings.UpdatedValue += UpdateMCName;
			CharacterManager.Instance.ActiveCharacterChanged += ActiveCharacterChanged;
			PlayerConfig.Instance.language.ValueChanged += ChangedLanguage;
			if (displayOnlyPlayerName)
				ChangeText(SaveSlot.ActiveSlot.PlayerName);
		}
		private void OnDisable()
		{
			CharacterManager.Instance.ActiveCharacterChanged -= ActiveCharacterChanged;
			SaveSlot.ActiveSlot.strings.UpdatedValue -= UpdateMCName;
			PlayerConfig.Instance.language.ValueChanged -= ChangedLanguage;
		}
		public void ChangeText(string unfilteredName)
		{
			string name = ReplaceText(unfilteredName);
			text.text = name;
		}
		private void ActiveCharacterChanged(Character? character)
		{
			if (!ShouldChangeName(character))
				return;
			ChangeText(character.Value.controller.CharacterNames.CurrentName);
		}
		private void ChangedLanguage(string newLanguage)
		{
			if (!ShouldChangeName(CharacterManager.Instance.ActiveCharacter))
				return;
			ChangeText(CharacterManager.Instance.ActiveCharacter.Value.controller.CharacterNames[newLanguage]);
		}
		private void UpdateMCName(string key, string oldValue, string newValue, Collection<string> source)
		{
			if (key != SaveSlot.KEY_PLAYER_NAME)
				return;
			TMP_Text text = GetComponent<TMP_Text>();
			text.text = ReplaceText(text.text);
		}

		public bool ShouldChangeName(Character? character)
		{
			if (!character.HasValue)
				return false;
			if (!displayOnlyPlayerName)
				return true;
			if (CharacterManager.Instance.ActiveCharacter.Value.controller.CharacterNames.CurrentName == SaveSlot.DEFAULT_NAME)
				return true;
			return false;
		}
	}
}