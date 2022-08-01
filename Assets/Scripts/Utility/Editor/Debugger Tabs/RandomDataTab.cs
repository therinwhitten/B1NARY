namespace B1NARY.Editor.Debugger
{
	using UnityEngine;
	using UnityEditor;

	public sealed class RandomDataTab : DebuggerTab
	{
		public override string Name => "Random Data";

		private bool showLimitValues = false;
		public override void DisplayTab()
		{
			showLimitValues = EditorGUILayout.ToggleLeft("Show 32bit limit count values", showLimitValues);
			EditorGUILayout.LabelField($"C# Random Iterations: {RandomFowarder.CSharpRandomIterations}");
			if (showLimitValues)
				EditorGUI.ProgressBar(BarRect, RandomFowarder.CSharpRandomIterations / uint.MaxValue, $"{RandomFowarder.CSharpRandomIterations}/{uint.MaxValue}");
			EditorGUILayout.LabelField($"Unity Random Iterations: {RandomFowarder.UnityRandomIterations}");
			if (showLimitValues)
				EditorGUI.ProgressBar(BarRect, RandomFowarder.UnityRandomIterations / uint.MaxValue, $"{RandomFowarder.UnityRandomIterations}/{uint.MaxValue}");
			EditorGUILayout.LabelField($"Doom Random Iterations: {RandomFowarder.DoomRandomIterations}");
			if (showLimitValues)
				EditorGUI.ProgressBar(BarRect, RandomFowarder.DoomRandomIterations / uint.MaxValue, $"{RandomFowarder.DoomRandomIterations}/{uint.MaxValue}");
			EditorGUI.indentLevel++;
			EditorGUILayout.LabelField($"Index: {RandomFowarder.DoomIndex}");
			EditorGUILayout.LabelField($"Next Value: {RandomFowarder.doomRandomTable[RandomFowarder.DoomIndex]}");
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
	}
}