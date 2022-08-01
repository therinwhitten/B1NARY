namespace B1NARY
{
	using UnityEngine;
	using DataPersistence;

	public static partial class GameCommands
	{
		// todo: add events so these would save automatically.
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
			PersistentData.LoadGame();
		}
	}
}