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
				return B1NARYResolution.MonitorResolutions
					.Where(resolution => resolution.RefreshRate == Screen.currentResolution.refreshRate)
					.OrderByDescending(resolution => (resolution.Width * 100) + resolution.Height)
					.Select(resolution => new KeyValuePair<string, Resolution>(resolution.ToString(), resolution))
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
			B1NARYResolution.ActiveResolution = (B1NARYResolution)CurrentValue;
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