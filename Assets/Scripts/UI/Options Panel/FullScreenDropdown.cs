namespace B1NARY.UI
{
	using System;
	using System.Collections.Generic;
	using UnityEngine;

	public sealed class FullScreenDropdown : DropdownPanel<FullScreenMode>
	{
		public override List<FullScreenMode> DefinedValues => new List<FullScreenMode>((FullScreenMode[])Enum.GetValues(typeof(FullScreenMode)));

		public override void PickedChoice(int index)
		{
			Resolution currentResolution = Screen.currentResolution;
			Screen.SetResolution(currentResolution.width, currentResolution.height, CurrentValue, currentResolution.refreshRate);
		}
	}
}