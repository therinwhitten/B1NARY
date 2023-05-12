#if UNITY_EDITOR
namespace B1NARY.Editor.Debugger
{
	using UnityEngine;
	using UnityEditor;
	
	public sealed class RandomDataTab : DebuggerTab
	{
		public override GUIContent Name => new GUIContent("Random Data");

		private bool showLimitValues = false;
		public override void DisplayTab()
		{
			showLimitValues = EditorGUILayout.ToggleLeft("Show unsigned 32bit limit count values", showLimitValues);
			EditorGUILayout.LabelField($"C# Random Iterations: {RandomForwarder.CSharpRandomIterations}");
			if (showLimitValues)
				EditorGUI.ProgressBar(BarRect, RandomForwarder.CSharpRandomIterations / uint.MaxValue, $"{RandomForwarder.CSharpRandomIterations}/{uint.MaxValue}");
			EditorGUILayout.LabelField($"Unity Random Iterations: {RandomForwarder.UnityRandomIterations}");
			if (showLimitValues)
				EditorGUI.ProgressBar(BarRect, RandomForwarder.UnityRandomIterations / uint.MaxValue, $"{RandomForwarder.UnityRandomIterations}/{uint.MaxValue}");
			EditorGUILayout.LabelField($"Doom Random Iterations: {RandomForwarder.DoomRandomIterations}");
			if (showLimitValues)
				EditorGUI.ProgressBar(BarRect, RandomForwarder.DoomRandomIterations / uint.MaxValue, $"{RandomForwarder.DoomRandomIterations}/{uint.MaxValue}");
			EditorGUI.indentLevel++;
			EditorGUILayout.LabelField($"Index: {RandomForwarder.DoomIndex}");
			EditorGUILayout.LabelField($"Next Value: {RandomForwarder.doomRandomTable[RandomForwarder.DoomIndex]}");
			EditorGUI.indentLevel--;
		}
		private Rect BarRect
		{
			get
			{
				Rect output = GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth, 20);
				output.x = 4;
				output.width = EditorGUIUtility.currentViewWidth - 8;
				return output;
			}
		}

		public override bool ConstantlyRepaint => false;
	}
}
#endif