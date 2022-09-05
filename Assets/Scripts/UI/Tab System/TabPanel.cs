namespace B1NARY
{
	using System;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.Events;
	using UnityEngine.UI;

	public class TabPanel : MonoBehaviour
	{
		public TabButton[] tabButtons;
		public int CurrentTabIndex { get; private set; } = 0;
		private void Awake()
		{
			for (int i = 0; i < tabButtons.Length; i++)
				tabButtons[i].Button.onClick.AddListener(() => InvokeListener(i));
		}
		private void InvokeListener(int index)
		{
			tabButtons[index].contents.SetActive(true);
			var subList = new List<TabButton>(tabButtons);
			subList.RemoveAt(index);
			for (int i = 0; i < subList.Count; i++)
			{
				if (subList[i].contents != null)
					subList[i].contents.SetActive(false);
			}
		}
	}
}