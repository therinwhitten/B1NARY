﻿namespace B1NARY.UI
{
	using B1NARY.DataPersistence;
	using B1NARY.Scripting;
	using System;
	using UnityEngine;

	public class OptionsMenuPauser : MonoBehaviour
	{
		public void OnEnable()
		{
			if (ScriptHandler.TryGetInstance(out var handler))
				handler.pauser.AddBlocker(this);
		}
		public void OnDisable()
		{
			if (ScriptHandler.TryGetInstance(out var handler))
				handler.pauser.RemoveBlocker(this);
			PlayerConfig.Instance.Save();
		}
	}
}