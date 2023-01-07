namespace B1NARY.UI
{
	using System.Linq;
	using System.Collections.Generic;
	using UnityEngine;

	public sealed class ResolutionsDropdown : DropdownPanel<Resolution>
	{
		public override List<KeyValuePair<string, Resolution>> DefinedPairs
		{
			get
			{
				return Screen.resolutions
					.OrderByDescending(resolution => (resolution.width * 100) + resolution.height)
					.Select(resolution => new KeyValuePair<string, Resolution>($"{resolution.width}x{resolution.height}", resolution))
					.ToList();
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
			Resolution resolution = CurrentValue;
			Screen.SetResolution(resolution.width, resolution.height,
				Screen.fullScreenMode, resolution.refreshRate);
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