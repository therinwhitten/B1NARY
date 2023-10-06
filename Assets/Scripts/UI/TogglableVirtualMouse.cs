namespace B1NARY.UI
{
	using System;
	using System.Collections;
	using System.Threading.Tasks;
	using UnityEngine;
	using UnityEngine.InputSystem;
	using UnityEngine.InputSystem.UI;

	public class TogglableVirtualMouse : VirtualMouseInput
	{
		[Header("Toggle Settings")]
		public PlayerInput inputSystem;
		public string gamepadTag = "Gamepad";

		private GameObject thisObj;
		CoroutineWrapper wrapper = null;

		private void Start()
		{
			thisObj = gameObject;
		}
		protected new void OnEnable()
		{
			base.OnEnable();
			//if (CoroutineWrapper.IsNotRunningOrNull(wrapper))
			//	wrapper = new CoroutineWrapper(inputSystem, Coroutine()).Start();
			//IEnumerator Coroutine()
			//{
			//	while (this != null)
			//	{
			//		yield return new WaitForEndOfFrame();
			//		ToggleFromGamepadCheck();
			//	}
			//}
		}
		protected new void OnDisable()
		{
			base.OnDisable();
		}
		protected void ToggleFromGamepadCheck()
		{
			bool output = inputSystem.currentControlScheme == gamepadTag;
			if (output == thisObj.activeSelf)
				return;
			thisObj.SetActive(output);
		}
	}
}