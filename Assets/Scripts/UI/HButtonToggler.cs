namespace B1NARY.UI
{
	using System;
	using UnityEngine;
	using UnityEngine.UI;

	[RequireComponent(typeof(Selectable))]
	public class HButtonToggler : MonoBehaviour
	{
		private static void FuckYouUnity1(HButtonToggler toggler, bool setting)
		{
			if (toggler == null)
				return;
			toggler.gameObject.SetActive(setting);
		}
		private static void FuckYouUnity2(HButtonToggler toggler, bool setting)
		{
			if (toggler == null)
				return;
			toggler.GetComponent<Selectable>().interactable = setting;
		}

		public bool totalToggle = false;
		private void Start()
		{
			if (totalToggle)
				PlayerConfig.Instance.hEnable.AttachValue(var => FuckYouUnity1(this, var));
			else
				PlayerConfig.Instance.hEnable.AttachValue(var => FuckYouUnity2(this, var));
		}
	}
}