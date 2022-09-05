namespace B1NARY
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using UnityEngine;
	using UnityEngine.Events;
	using UnityEngine.UI;

	public class TabPanel : MonoBehaviour
	{
		public TabButton[] tabButtons;
		private Dictionary<int, TabButton> validTabs;
		public int CurrentTabIndex { get; private set; } = 0;
		private void Start()
		{
			validTabs = new Dictionary<int, TabButton>(tabButtons.Length);
			for (int i = 0; i < tabButtons.Length; i++)
			{
				if (tabButtons[i] == null)
				{
					Debug.Log($"element {i} in {name} doesn't lead to a {nameof(TabButton)}.");
					continue;
				}
				validTabs.Add(i, tabButtons[i]);
				validTabs[i].Button.onClick.AddListener(() => InvokeListener(i));
			}
		}
		private void InvokeListener(int index)
		{
			if (validTabs[index].contents != null)
				validTabs[index].contents.SetActive(true);
			var activePair = validTabs.Single(pair => pair.Key == index);
			var subList = new List<KeyValuePair<int, TabButton>>(validTabs);
			subList.Remove(activePair);
			for (int i = 0; i < subList.Count; i++)
				if (subList[i].Value.contents != null)
					subList[i].Value.contents.SetActive(false);
		}
	}
}