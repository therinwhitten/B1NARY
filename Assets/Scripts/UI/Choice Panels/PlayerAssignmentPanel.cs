namespace B1NARY.UI
{
	using B1NARY.DataPersistence;
	using TMPro;
	using UnityEngine;
	using UnityEngine.UI;

	public class PlayerAssignmentPanel : MonoBehaviour
	{
		public TMP_InputField nameInput;
		public Button male, female;
		private Gender gender = Gender.Male;
		private void Start()
		{
			male.onClick.AddListener(() => gender = Gender.Male);
			female.onClick.AddListener(() => gender = Gender.Female);
		}
		public void OnDisable()
		{
			SaveSlot.ActiveSlot.PlayerName = nameInput.text;
			SaveSlot.ActiveSlot.Gender = gender;
		}
	}
}