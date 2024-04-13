using UnityEngine;
using TMPro;
using B1NARY.CharacterManagement;
using B1NARY.DataPersistence;
using System;
using System.Collections.Generic;

namespace B1NARY.UI
{
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
        public float transitionSpeed = 10f; // Adjust the transition speed as needed

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
            StartCoroutine(ScaleText(0f, 1f, transitionSpeed)); // Scale from 0 to 1
            string name = ReplaceText(unfilteredName);
            text.text = name;
        }

        private void ActiveCharacterChanged(Character? character)
        {
            if (!ShouldChangeName(character))
                return;

            StartCoroutine(ScaleText(1f, 0f, transitionSpeed)); // Scale from 1 to 0
            ChangeText(character.Value.controller.CharacterNames.CurrentName);
        }

        private void ChangedLanguage(string newLanguage)
        {
            if (!ShouldChangeName(CharacterManager.Instance.ActiveCharacter))
                return;

            StartCoroutine(ScaleText(1f, 0f, transitionSpeed)); // Scale from 1 to 0
            ChangeText(CharacterManager.Instance.ActiveCharacter.Value.controller.CharacterNames[newLanguage]);
        }

        private void UpdateMCName(string key, string oldValue, string newValue, Collection<string> source)
        {
            if (key != SaveSlot.KEY_PLAYER_NAME)
                return;

            StartCoroutine(ScaleText(1f, 0f, transitionSpeed)); // Scale from 1 to 0
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

        private System.Collections.IEnumerator ScaleText(float startScale, float endScale, float speed)
        {
            float t = 0f;
            Vector3 startScaleVec = new Vector3(startScale, startScale, startScale);
            Vector3 endScaleVec = new Vector3(endScale, endScale, endScale);

            while (t < 1f)
            {
                t += Time.deltaTime * speed;
                text.rectTransform.localScale = Vector3.Lerp(startScaleVec, endScaleVec, t);
                yield return null;
            }

            text.rectTransform.localScale = endScaleVec; // Ensure the final scale is set
        }
    }
}
