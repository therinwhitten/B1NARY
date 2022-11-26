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
		public const string IsBinaryKey = "n-b";

		public Button binaryButton, nonBinaryButton;
		private void Awake()
		{
			binaryButton.onClick.AddListener(() => OnBinaryClick(true));
			nonBinaryButton.onClick.AddListener(() => OnBinaryClick(false));
			void OnBinaryClick(bool isBinary)
			{
				PersistentData.Instance.Booleans[IsBinaryKey] = isBinary;
			}
		}
	}
}