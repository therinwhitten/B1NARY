namespace B1NARY.UI
{
	using System.Linq;
	using System.Collections.Generic;
	using UnityEngine;

	public sealed class ResolutionsDropdown : DropdownPanel<Resolution>
	{
		public override List<string> Visuals => Values
			.Select(res => $"{res.width}x{res.height} {res.refreshRate}Hz")
			.Reverse()
			.ToList();
		public override List<Resolution> DefinedValues =>
			Screen.resolutions
			.Where(resolution => resolution.refreshRate == Screen.currentResolution.refreshRate)
			.Reverse().ToList();
		public override int InitialValue => Values.IndexOf(Screen.currentResolution);

		protected override void PickedChoice(int index)
		{
			Resolution resolution = CurrentValue;
			Screen.SetResolution(resolution.width, resolution.height,
				Screen.fullScreenMode, resolution.refreshRate);
		}
	}
}