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
		private void Start()
		{
			inputSystem.StartCoroutine(Coroutine());
			IEnumerator Coroutine()
			{
				while (this != null)
				{
					ToggleFromGamepadCheck();
					yield return new WaitForEndOfFrame();
				}
			}
		}
		protected new void OnEnable()
		{
			base.OnEnable();
		}
		protected new void OnDisable()
		{
			base.OnDisable();
		}
		protected void ToggleFromGamepadCheck()
		{
			this.enabled = inputSystem.currentControlScheme == gamepadTag;
		}
	}
}