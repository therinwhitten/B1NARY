namespace B1NARY.UI
{
	using B1NARY.DataPersistence;
	using B1NARY.DesignPatterns;
	using B1NARY.Scripting;
	using System;
	using UnityEngine;
	using UnityEngine.Events;

	public class OptionsMenuPauser : Singleton<OptionsMenuPauser>
	{
		public UnityEvent onEnable, onDisable;

		private void OnEnable()
		{
			if (ScriptHandler.TryGetInstance(out var handler))
				handler.pauser.AddBlocker(this);
			onEnable.Invoke();
		}
		private void OnDisable()
		{
			if (ScriptHandler.TryGetInstance(out var handler))
				handler.pauser.RemoveBlocker(this);
			PlayerConfig.Instance.Save();
			onDisable.Invoke();
		}
	}
}