﻿namespace B1NARY.Scripting
{
	using B1NARY.DesignPatterns;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using UnityEngine.InputSystem;
	using UnityEngine.UI;

	public class ButtonInvoker : Singleton<ButtonInvoker>
	{
		public static CommandArray Commands = new()
		{
			["pressbutton"] = ((Action<string>)((name) =>
			{
				InstanceOrDefault.ActivateButton(name);
			})),
		};

		private Dictionary<string, Button> AllButtons = null;
		private void Start()
		{
			if (AllButtons is null)
				DefineAllButtons();
		}
		public void ActivateButton(string buttonName)
		{
			if (AllButtons is null)
				DefineAllButtons();
			AllButtons[buttonName].onClick.Invoke();
		}

		private void DefineAllButtons()
		{
			AllButtons = FindObjectsOfType<Button>().ToDictionary(button => button.name);
		}
	}
}