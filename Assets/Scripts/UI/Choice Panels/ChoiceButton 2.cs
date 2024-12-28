namespace B1NARY.UI
{
	using System;
	using UnityEngine;
	using UnityEngine.EventSystems;
	using UnityEngine.UI;
	using B1NARY.Audio;
	using TMPro;
	using B1NARY.Scripting;
	using B1NARY.CharacterManagement;

	[RequireComponent(typeof(Button))]
	public class ChoiceButton : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
	{
		public VoiceActorHandler VoiceActor;
		public ScriptLine currentLine;

		public string Text
		{
			get
			{
				CheckForNullText();
				return m_text.text;
			}
			set
			{
				CheckForNullText();
				m_text.text = value;
			}
		}
		private TMP_Text m_text;
		private void CheckForNullText()
		{
			if (m_text == null)
				m_text = GetComponentInChildren<TMP_Text>(true);
		}

		public ChoicePanel tiedPanel;

		public void OnPointerEnter(PointerEventData eventData)
		{
			VoiceActor.Play(currentLine);
		}
		public void OnPointerClick(PointerEventData eventData)
		{
			if (tiedPanel == null)
				return;
			tiedPanel.HandlePress(currentLine);
		}
	}
}