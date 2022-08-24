namespace B1NARY.UI
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Linq;
	using UnityEngine;
	using UnityEngine.UI;
	using static B1NARY.RandomFowarder;

	[RequireComponent(typeof(Text))]
	public class LoadingScreenTips : MonoBehaviour
	{
		public static readonly ReadOnlyCollection<string> loadingScreenTips = Array.AsReadOnly(new string[]
		{
			"If you want to eat something, chew it until its mushy and swallow.",
			"You are breathing manually now.",
			"its possible to count up to 1024 with your fingers using binary, or base 2!",
			"Strike the weakpoint for massive damage!",
		});

		[SerializeField] private RandomType randomType = RandomType.Unity;
		private Text text;
		private Coroutine changeNewTextCoroutine;
		private void Awake()
		{
			text = GetComponent<Text>();
		}
		private void OnEnable()
		{
			text.text = loadingScreenTips[Next(loadingScreenTips.Count, randomType)];
			changeNewTextCoroutine = StartCoroutine(CoroutineDelay());
		}
		private IEnumerator CoroutineDelay()
		{
			while (true)
			{
				yield return new WaitForSeconds(5f);
				// removes existing tip.
				string[] subCollection = loadingScreenTips.Where(str => str != text.text).ToArray();
				text.text = subCollection[Next(subCollection.Length, randomType)];
			}
		}
		private void OnDisable()
		{
			StopCoroutine(changeNewTextCoroutine);
		}
	}
}