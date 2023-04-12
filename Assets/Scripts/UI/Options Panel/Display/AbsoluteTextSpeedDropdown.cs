namespace B1NARY.UI
{
	using System.Linq;
	using System.Collections.Generic;
	using UnityEngine;
	using System;

	public sealed class AbsoluteTextSpeedDropdown : DropdownPanel<int>
	{
		public override List<KeyValuePair<string, int>> DefinedPairs => new List<KeyValuePair<string, int>>
		{
			new KeyValuePair<string, int>("Slow", 45),
			new KeyValuePair<string, int>("Normal", 30),
			new KeyValuePair<string, int>("Fast", 15),
			new KeyValuePair<string, int>("Immediate", 0),
		};
		public override int InitialValue
		{
			get
			{
				for (int i = 0; i < Pairs.Count; i++)
					if (PlayerConfig.Instance.dialogueSpeedTicks.Value == Pairs[i].Value)
						return i;
				throw new IndexOutOfRangeException($"No index value that matches {PlayerConfig.Instance.dialogueSpeedTicks.Value}!");
			}
		}
		protected override void PickedChoice(int index)
		{
			base.PickedChoice(index);
			PlayerConfig.Instance.dialogueSpeedTicks.Value = CurrentValue;
		}
	}
}
#if UNITY_EDITOR
namespace B1NARY.UI.Editor
{
	using UnityEngine;
	using UnityEditor;

	[CustomEditor(typeof(AbsoluteTextSpeedDropdown))]
	public sealed class AbsoluteTextSpeedDropdownEditor : DropDownEditor<int>
	{

	}
}
#endif