namespace HDConsole
{
	using System.Collections;
	using TMPro;
	using UnityEngine;
	using UnityEngine.UI;

	internal class FunnyHDConsoleTitles : MonoBehaviour
	{
		public TMP_Text title;
		public string titleText = "HDConsole";
		public HorizontalLayoutGroup reload;
		public string[] additives = { "" };
		public float funnyChance = 0.2f;

		private void Reset()
		{
			title = GetComponent<TMP_Text>();
		}

		private void OnEnable()
		{
			string @base = $"<b>{titleText}</b>";
			if (Random.value > funnyChance)
				title.text = @base;
			else
				title.text = $"{@base}: <i>{additives[Random.Range(0, additives.Length)]}</i>";
		}
	}
}
