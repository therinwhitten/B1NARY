namespace B1NARY.UI.Gamepads
{
	using B1NARY.DesignPatterns;
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using UnityEngine;
	using UnityEngine.EventSystems;
	using UnityEngine.InputSystem;
	using UnityEngine.InputSystem.UI;
	using UnityEngine.UI;

	public class AutoGamepadButtonSelector : SoftMultiton<AutoGamepadButtonSelector>
	{
		internal CoroutineWrapper activeCoroutine;

		public Selectable selectable;
		public bool forceSelection = false;
		public bool selectWhenStarted = false;
		public bool selectWhenEnabled = true;

		protected virtual void Reset()
		{
			selectable = GetComponent<Selectable>();
		}

		private void Start()
		{
			if (selectWhenStarted)
				SelectDelay();
		}
		protected override void MultitonEnable()
		{
			if (selectWhenEnabled)
				SelectDelay();
		}
		IEnumerator Delay()
		{
			yield return new WaitForEndOfFrame();
			Select();
		}

		public void Select()
		{
			if (forceSelection || GamepadAutoSelector.Instance.IsGamepadEnabled)
				GamepadAutoSelector.Instance.eventSystem.SetSelectedGameObject(gameObject);
		}
		public void SelectDelay()
		{
			if (!CoroutineWrapper.IsNotRunningOrNull(activeCoroutine))
				activeCoroutine.Stop();
			activeCoroutine = new CoroutineWrapper(this, Delay()).Start();
		}
	}
}
