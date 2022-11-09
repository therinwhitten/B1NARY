namespace B1NARY.UI
{
	using B1NARY.DataPersistence;
	using UnityEngine;
	using UnityEngine.UI;

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
				if (PersistentData.Instance.bools.ContainsKey(IsBinaryKey))
					PersistentData.Instance.bools[IsBinaryKey] = isBinary;
				PersistentData.Instance.bools.Add(IsBinaryKey, isBinary);
			}
		}
	}
}