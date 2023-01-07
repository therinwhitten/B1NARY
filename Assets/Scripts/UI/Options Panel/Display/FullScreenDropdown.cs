namespace B1NARY.UI
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using UnityEngine;

	public sealed class FullScreenDropdown : DropdownPanel<FullScreenMode>
	{
		public override List<KeyValuePair<string, FullScreenMode>> DefinedPairs => new List<KeyValuePair<string, FullScreenMode>>(3)
		{
			new KeyValuePair<string, FullScreenMode>("Fullscreen", FullScreenMode.ExclusiveFullScreen),
			new KeyValuePair<string, FullScreenMode>("Borderless", FullScreenMode.FullScreenWindow),
			new KeyValuePair<string, FullScreenMode>("Windowed", FullScreenMode.Windowed)
		};
		public override int InitialValue => Values.ToList().IndexOf(Screen.fullScreenMode);

		protected override void PickedChoice(int index)
		{
			base.PickedChoice(index);
			Resolution currentResolution = Screen.currentResolution;
			Screen.SetResolution(currentResolution.width, currentResolution.height, CurrentValue, currentResolution.refreshRate);
		}
	}
}
#if UNITY_EDITOR
namespace B1NARY.UI.Editor
{
	using UnityEngine;
	using UnityEditor;

	[CustomEditor(typeof(FullScreenDropdown))]
	public sealed class FullScreenDropdownEditor : DropDownEditor<FullScreenMode>
	{

	}
}
#endif