namespace B1NARY.UI.Editor
{
	using UnityEngine;
	using UnityEditor;

	[CustomEditor(typeof(ResolutionsDropdown))]
	public sealed class ResolutionsDropdownEditor : DropDownEditor<Resolution>
	{

	}
	[CustomEditor(typeof(FullScreenDropdown))]
	public sealed class FullScreenDropdownEditor : DropDownEditor<FullScreenMode>
	{

	}
}