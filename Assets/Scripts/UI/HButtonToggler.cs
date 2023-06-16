namespace B1NARY.UI
{
	using System;
	using UnityEngine;
	using UnityEngine.UI;

	[RequireComponent(typeof(Selectable))]
	public class HButtonToggler : MonoBehaviour
	{
		public bool totalToggle = false;
		private void Start()
		{
			if (totalToggle)
				PlayerConfig.Instance.hEnable.AttachValue(var => gameObject.SetActive(var));
			else
				PlayerConfig.Instance.hEnable.AttachValue(var => GetComponent<Selectable>().interactable = var);
		}
	}
}