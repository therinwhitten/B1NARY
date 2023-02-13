namespace B1NARY.UI
{
	using B1NARY.DataPersistence;
	using UnityEngine;
	using UnityEngine.UI;

	/// <summary>
	/// a simple panel/component that adds functionality to the binary options 
	/// in B1NARY's binary panel.
	/// </summary>
	public class BinaryB1NARYPanelBehaviour : MonoBehaviour
	{
		public string BinaryKeyName = "n-b";

		public Button binaryButton, nonBinaryButton;
		private void Awake()
		{
			binaryButton.onClick.AddListener(OnBinaryClick);
			nonBinaryButton.onClick.AddListener(OnNonBinaryClick);
		}
		private void OnBinaryClick()
		{
			SaveSlot.ActiveSlot.ScriptDocumentInterface.bools[BinaryKeyName] = true;
			gameObject.SetActive(false);
		}
		private void OnNonBinaryClick()
		{
			SaveSlot.ActiveSlot.ScriptDocumentInterface.bools[BinaryKeyName] = false;
			gameObject.SetActive(false);
		}
	}
}