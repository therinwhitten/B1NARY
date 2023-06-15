namespace B1NARY.UI
{
	using System;
	using UnityEngine;
	using UnityEngine.UI;

	[RequireComponent(typeof(Selectable))]
	public class HButtonToggler : MonoBehaviour
	{
		private void Start()
		{
			PlayerConfig.Instance.hEnable.AttachValue(var => GetComponent<Selectable>().interactable = var);
		}
	}
}