namespace B1NARY.UI
{
	using System;
	using UnityEngine;

	public sealed class FullScreenDropdown : DropdownPanel<FullScreenMode>
	{
		public new FullScreenMode[] values = (FullScreenMode[])Enum.GetValues(typeof(FullScreenMode));

		public override void PickedChoice(int index)
		{
			throw new NotImplementedException();
		}
	}
}