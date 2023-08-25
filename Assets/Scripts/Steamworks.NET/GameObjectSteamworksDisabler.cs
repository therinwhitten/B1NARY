namespace B1NARY.Steamworks
{
	using System;
	using UnityEngine;

	public sealed class GameObjectSteamworksDisabler : MonoBehaviour
	{
		public bool invert = false;
		private void Start()
		{
#if DISABLESTEAMWORKS
			bool disable = true;
#else
			bool disable = false;
#endif
			if (invert)
				disable = !disable;
			gameObject.SetActive(disable);
		}
	}
}
