namespace B1NARY.UI
{
	using B1NARY.DataPersistence;
	using UnityEngine;
	using UnityEngine.UI;

	[RequireComponent(typeof(Button))]
	public sealed class SavePanelButtonToggler : MonoBehaviour
	{
		private Button button;

		private void Awake()
		{
			button = GetComponent<Button>();
		}
		private void OnEnable()
		{
			button.interactable = SaveSlot.Instance != null;
		}
	}
}