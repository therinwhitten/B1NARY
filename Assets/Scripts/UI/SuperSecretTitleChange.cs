﻿namespace B1NARY.UI
{
	using System;
	using TMPro;
	using UnityEngine;

	public sealed class SuperSecretTitleChange : MonoBehaviour
	{
		public TMP_Text title;
		private void Awake()
		{
			if (RandomFowarder.Next(0, 1000) == 0)
				title.text = "B1NARDY";
		}
	}
}