namespace B1NARY.UI
{
	using System.Linq;
	using System.Collections.Generic;
	using UnityEngine;

	public sealed class ResolutionsDropdown : DropdownPanel<Resolution>
	{
		public override List<string> Visuals
		{
			get
			{
				var output = new List<string>(Values.Count);
				for (int i = 0; i < Values.Count; i++)
					output.Add($"{Values[i].width}x{Values[i].height} {Values[i].refreshRate}Hz");
				return output;
			}
		}

		public override List<Resolution> DefinedValues
		{
			get
			{
				Resolution[] resolutionArray = Screen.resolutions;
				var resolutions = new LinkedList<Resolution>();
				for (int i = 0; i < resolutionArray.Length; i++)
					if (resolutionArray[i].refreshRate == Screen.currentResolution.refreshRate)
						resolutions.AddFirst(resolutionArray[i]);
				return new List<Resolution>(resolutions);
			}
		}

		public override int InitialValue => Values.IndexOf(Screen.currentResolution);

		protected override void PickedChoice(int index)
		{
			Resolution resolution = CurrentValue;
			Screen.SetResolution(resolution.width, resolution.height,
				Screen.fullScreenMode, resolution.refreshRate);
		}
	}
}