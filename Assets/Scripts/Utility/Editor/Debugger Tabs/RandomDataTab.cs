using UnityEngine;
using UnityEditor;

public sealed class RandomDataTab : DebuggerTab
{
	public override string Name => "Random Data";

	public override void DisplayTab()
	{
		EditorGUILayout.LabelField($"C# Random Iterations: {RandomFowarder.CSharpRandomIterations}");
		EditorGUI.ProgressBar(BarRect, RandomFowarder.CSharpRandomIterations / uint.MaxValue, $"{RandomFowarder.CSharpRandomIterations}/{uint.MaxValue}");
		EditorGUILayout.LabelField($"Unity Random Iterations: {RandomFowarder.UnityRandomIterations}");
		EditorGUI.ProgressBar(BarRect, RandomFowarder.UnityRandomIterations / uint.MaxValue, $"{RandomFowarder.UnityRandomIterations}/{uint.MaxValue}");
		EditorGUILayout.LabelField($"Doom Random Iterations: {RandomFowarder.DoomRandomIterations}");
		EditorGUI.ProgressBar(BarRect, RandomFowarder.DoomRandomIterations / uint.MaxValue, $"{RandomFowarder.DoomRandomIterations}/{uint.MaxValue}");
		EditorGUILayout.LabelField($"Index: {RandomFowarder.DoomIndex}");
		EditorGUILayout.LabelField($"Next Value: {RandomFowarder.doomRandomTable[RandomFowarder.DoomIndex]}");
	}
	private Rect BarRect { get
		{
			Rect output = GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth, 20);
			output.x = 4;
			output.width = EditorGUIUtility.currentViewWidth - 8;
			return output;
		}
	}
}