namespace B1NARY.DataPersistence
{
	using UnityEngine;
	using UnityEngine.InputSystem;

	public sealed class DataCommands : MonoBehaviour
	{
		public PlayerInput input;
		public string saveButton = "QuickSave";
		public string loadButton = "LoadSave";

		private void OnEnable()
		{
			input.actions.FindAction(saveButton, true).performed += SaveGame;
			input.actions.FindAction(loadButton, true).performed += LoadGame;
		}
		private void OnDisable()
		{
			input.actions.FindAction(saveButton, true).performed -= SaveGame;
			input.actions.FindAction(loadButton, true).performed -= LoadGame;
		}
		private void SaveGame(InputAction.CallbackContext context)
		{
			PersistentData.SaveGame();
		}
		private void LoadGame(InputAction.CallbackContext context)
		{
			PersistentData.LoadGame();
		}
	}
}