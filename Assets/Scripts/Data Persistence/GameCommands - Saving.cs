namespace B1NARY.DataPersistence
{
	using UnityEngine;
	using UnityEngine.InputSystem;

	public static partial class GameCommands
	{
		private static PlayerInput m_playerInput;
		public static PlayerInput PlayerInput
		{
			get
			{
				if (m_playerInput == null)
					m_playerInput = Object.FindObjectOfType<PlayerInput>();
				return m_playerInput;
			}
			set => m_playerInput = value;
		}

		[ExecuteAlways]
		private static void InputSavingConstructor()
		{

		}

		private static void OnSave()
		{
			PersistentData.SaveGame();
		}
		private static void OnLoad()
		{
			_ = PersistentData.LoadGame();
		}
	}
}