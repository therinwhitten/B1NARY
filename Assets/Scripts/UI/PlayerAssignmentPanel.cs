namespace B1NARY.UI
{
	using B1NARY.DataPersistence;
	using TMPro;
	using UnityEngine;
	using UnityEngine.UI;

	public class PlayerAssignmentPanel : MonoBehaviour
	{
		public string MaleAssignmentKey = "MalePath";


		public TMP_InputField nameInput;
		public Button male, female;
		private bool isFemale = false;
		private void MaleTrue() => isFemale = false;
		private void FemaleTrue() => isFemale = true;

		private void Awake()
		{
			male.onClick.AddListener(MaleTrue);
			female.onClick.AddListener(FemaleTrue);
		}

		public void OnDisable()
		{
			SaveSlot.ActiveSlot.PlayerName = nameInput.text;
			SaveSlot.ActiveSlot.booleans[MaleAssignmentKey] = !isFemale;
		}
	}
}