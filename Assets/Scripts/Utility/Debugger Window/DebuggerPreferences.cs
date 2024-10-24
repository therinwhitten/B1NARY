﻿#if UNITY_EDITOR
namespace B1NARY.Editor.Debugger
{
	using System.Collections.Generic;
	using UnityEditor;

	public sealed class DebuggerPreferences :
		Dictionary<DebuggerPreferences.DataType, List<(string name, object @default)>>
	{
		public enum DataType
		{
			Bool,
			Int,
			Float,
			String,
			StringPopup,
		}
	}
}
#endif