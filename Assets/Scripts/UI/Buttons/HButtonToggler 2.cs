namespace B1NARY.UI
{
	using B1NARY.DataPersistence;
	using System;
	using UnityEngine;
	using UnityEngine.UI;

	[RequireComponent(typeof(Selectable))]
	public class HButtonToggler : MonoBehaviour
	{
		private static void DisableObject(HButtonToggler toggler, bool setting)
		{
			if (toggler == null)
				return;
			toggler.gameObject.SetActive(setting);
		}
		private static void DisableBootun(HButtonToggler toggler, bool setting)
		{
			if (toggler == null)
				return;
			toggler.GetComponent<Selectable>().interactable = setting;
		}

		public bool totalToggle = false;
		private void Start()
		{
			if (totalToggle)
				PlayerConfig.Instance.hEnable.AttachValue(var => DisableObject(this, var));
			else
				PlayerConfig.Instance.hEnable.AttachValue(var => DisableBootun(this, var));
		}
	}
}