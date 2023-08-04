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
		public Button binaryButton, nonBinaryButton;
		private void Start()
		{
			binaryButton.onClick.AddListener(() => Commit(true));
			nonBinaryButton.onClick.AddListener(() => Commit(false));
			void Commit(bool isBinary)
			{
				SaveSlot.ActiveSlot.Slot.IsBinary = isBinary;
				gameObject.SetActive(false);
			}
		}
	}
}