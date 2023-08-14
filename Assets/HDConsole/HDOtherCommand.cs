namespace HDConsole
{
	using System;
	using TMPro;
	using UnityEditor;
	using UnityEngine;
	using UnityEngine.UI;

	public class HDOtherCommand : MonoBehaviour
	{
		public TMP_Text text;
		public Button button;

		private void Reset()
		{
			text = GetComponent<TMP_Text>();
			button = GetComponent<Button>();
		}

		public HDOtherCommand DuplicateTo(RectTransform transform)
		{
			GameObject duplicate = Instantiate(gameObject);
			duplicate.SetActive(true);
			duplicate.transform.SetParent(transform);
			return duplicate.GetComponent<HDOtherCommand>();
		}

		public void Destroy() => Destroy(gameObject);
	}
}