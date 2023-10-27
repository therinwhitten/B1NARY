namespace B1NARY.UI
{
	using System;
	using TMPro;
	using UnityEngine;
	using UnityEngine.UI;

	/// <summary>
	/// Handles the interfaces to confirm or deny anything
	/// </summary>
	public class InterfaceHandler : MonoBehaviour
	{
		[SerializeField]
		private Button confirm;
		[SerializeField]
		private Button cancel;
		public TMP_Text text;

		private void Invoke(bool type)
		{
			OnPress?.Invoke(type);
			OnPress = null;
			gameObject.SetActive(false);
		}
		private void Start()
		{
			confirm.onClick.AddListener(() => Invoke(true));
			cancel.onClick.AddListener(() => Invoke(false));
		}
		private void OnDisable()
		{
			OnPress = null;
		}
		public event Action<bool> OnPress;
	}
}