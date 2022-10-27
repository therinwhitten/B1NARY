namespace B1NARY.UI
{
	using B1NARY.DataPersistence;
	using TMPro;
	using UnityEngine;
	using UnityEngine.UI;

	public class PlayerAssignmentPanel : MonoBehaviour
	{
		public string isMaleDocument = "MalePath";


		public TMP_InputField nameInput;
		public Button male, female;
		private bool isFemale = false;

		private void Awake()
		{
			male.onClick.AddListener(() => isFemale = false);
			female.onClick.AddListener(() => isFemale = true);
		}

		public void Complete()
		{
			PersistentData.playerName = nameInput.text;
			if (PersistentData.bools.ContainsKey(isMaleDocument))
				PersistentData.bools[isMaleDocument] = !isFemale;
			PersistentData.bools.Add(isMaleDocument, !isFemale);
		}
	}
}