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
			PersistentData.Instance.playerName = nameInput.text;
			if (PersistentData.Instance.bools.ContainsKey(isMaleDocument))
				PersistentData.Instance.bools[isMaleDocument] = !isFemale;
			PersistentData.Instance.bools.Add(isMaleDocument, !isFemale);
		}
	}
}