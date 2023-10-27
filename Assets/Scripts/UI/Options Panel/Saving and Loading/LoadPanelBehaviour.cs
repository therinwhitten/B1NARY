namespace B1NARY.UI.Saving
{
	using B1NARY.DataPersistence;
	using System;
	using TMPro;
	using UnityEngine;
	using UnityEngine.UI;

	public class LoadPanelBehaviour : MonoBehaviour
	{
		public Image foregroundImage;
		public TMP_Text tmpText;
		public Button button;
	}

	public struct LoadSlotInstance
	{
		public LoadPanelBehaviour target;
		public SaveSlot source;
	}
}