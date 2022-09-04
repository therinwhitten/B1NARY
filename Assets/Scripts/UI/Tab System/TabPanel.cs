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
		public int CurrentTabIndex { get; private set; }
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
			subList.ForEach(tab => tab.contents.SetActive(false));
		}
	}
}