namespace B1NARY.UI
{
	using System;
	using TMPro;
	using UnityEngine;

	public sealed class SuperSecretTitleChange : MonoBehaviour
	{
		public TMP_Text title;
		private void Start()
		{
			if (RandomForwarder.Next(0, 1000) == 0)
				title.text = "B1NARDY";
			else if (RandomForwarder.Next(0, 1000) == 0)
				title.text = "HEX6DEC1MAL";
		}
		private void Reset()
		{
			title = GetComponent<TMP_Text>();
		}
	} 
}