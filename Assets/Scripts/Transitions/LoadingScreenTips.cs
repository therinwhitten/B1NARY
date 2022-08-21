namespace B1NARY.UI
{
	using System;
	using System.Collections.ObjectModel;
	using UnityEngine;
	using UnityEngine.UI;
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
		public static string PickNewRandomLoadingScreenTip(RandomFowarder.RandomType randomType)
			=> loadingScreenTips[RandomFowarder.Next(loadingScreenTips.Count, randomType)];

		private Text text;
		private void Awake()
		{
			text = GetComponent<Text>();
		}
		private void Start()
		{
			text.text = PickNewRandomLoadingScreenTip(RandomFowarder.RandomType.Unity);
		}
	}
}