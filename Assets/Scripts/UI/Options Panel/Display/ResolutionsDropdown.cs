namespace B1NARY.UI
{
	using System.Linq;
	using System.Collections.Generic;
	using UnityEngine;

	public sealed class ResolutionsDropdown : DropdownPanel<Resolution>
	{
		public static KeyValuePair<string, Resolution> ResolutionDisplay(Resolution resolution)
			=> new($"{resolution.width}p x {resolution.height}p", resolution);
		public override List<KeyValuePair<string, Resolution>> DefinedPairs
		{
			get
			{
				List<KeyValuePair<string, Resolution>> pairs = new(ResolutionUtility.OrderedResolutions.Count);
				for (int i = 0; i < ResolutionUtility.OrderedResolutions.Count; i++)
					pairs.Add(ResolutionDisplay(ResolutionUtility.OrderedResolutions[i]));
				return pairs;
			}
		}

		protected override void Awake()
		{
			base.Awake();
		}
		public override int InitialValue => Values.ToList().IndexOf(Screen.currentResolution);

		protected override void PickedChoice(int index)
		{
			base.PickedChoice(index);
			Screen.SetResolution(CurrentValue.width, CurrentValue.height, Screen.fullScreenMode, CurrentValue.refreshRateRatio);
		}
	}
}
#if UNITY_EDITOR
namespace B1NARY.UI.Editor
{
	using UnityEngine;
	using UnityEditor;

	[CustomEditor(typeof(ResolutionsDropdown))]
	public sealed class ResolutionsDropdownEditor : DropDownEditor<Resolution>
	{

	}
}
#endif