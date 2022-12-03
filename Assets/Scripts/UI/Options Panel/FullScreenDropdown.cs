namespace B1NARY.UI
{
	using System;
	using System.Collections.Generic;
	using UnityEngine;

	public sealed class FullScreenDropdown : DropdownPanel<FullScreenMode>
	{
		public override List<string> Visuals { get; } = new List<string>(3)
		{
			"Fullscreen",
			"Borderless",
			"Windowed"
		};
		public override List<FullScreenMode> DefinedValues => new List<FullScreenMode>(3)
		{
			FullScreenMode.ExclusiveFullScreen,
			FullScreenMode.FullScreenWindow,
			FullScreenMode.Windowed
		};
		public override int InitialValue => Values.IndexOf(Screen.fullScreenMode);

		protected override void PickedChoice(int index)
		{
			Resolution currentResolution = Screen.currentResolution;
			Screen.SetResolution(currentResolution.width, currentResolution.height, CurrentValue, currentResolution.refreshRate);
		}
	}
}