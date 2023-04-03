namespace B1NARY.UI
{
	using B1NARY.Scripting;
	using System;
	using UnityEngine;

	public sealed class OptionsPanel : MonoBehaviour
	{
		public void OnEnable()
		{
			if (ScriptHandler.HasInstance)
				ScriptHandler.Instance.pauser?.Pause();
		}
		public void OnDisable()
		{
			if (ScriptHandler.HasInstance)
				ScriptHandler.Instance.pauser?.Play();
		}
	}
}