namespace B1NARY.UI.Saving
{
	using B1NARY.DataPersistence;
	using System;
	using TMPro;
	using UnityEngine;
	using UnityEngine.UI;

	/// <summary>
	/// Handles the 'boxes' for representing saves.
	/// </summary>
	public class SavePanelBehaviour : MonoBehaviour
	{
		public Image foregroundImage;
		public TMP_Text tmpText;
		public Button button;

		/// <summary>
		/// Possibly null if not present.
		/// </summary>
		[Space]
		public Button deleteButton;
	}
	public struct SaveSlotInstance
	{
		public SavePanelBehaviour target;
		public SaveSlot source;
	}
}