using System;
using UnityEngine;
using UnityEditor;

public sealed class AboutTab : DebuggerTab
{
	private static readonly string[] changes =
	{

	};
	public override string Name => "About";
	public override void DisplayTab()
	{
		EditorGUILayout.LabelField("Version 0.1.1b");
		if (changes.Length <= 0)
			return;
		EditorGUILayout.LabelField("Changes:", EditorStyles.boldLabel);
		EditorGUI.indentLevel++;
		foreach (string i in changes)
			EditorGUILayout.LabelField(i);
		EditorGUI.indentLevel--;


	}

	public override int Order => int.MinValue + 1;


	// https://www.techtalk7.com/calculate-a-ratio-in-c/
	/*
	private static Vector2Int Ratio(int a, int b)
	{
		int gcd = GCD(a, b);
		return new Vector2Int(a / gcd, b / gcd);
	}
	private static int GCD(int a, int b) => b == 0 ? Math.Abs(a) : GCD(b, a % b);
	*/
}