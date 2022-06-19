using System;
using UnityEngine;
using UnityEditor;

public sealed class AboutTab : DebuggerTab
{
	public override string Name => "About";

	private Texture2D iconImageForAbout;
	public override void DisplayTab()
	{
		EditorGUILayout.LabelField("Debugger Created and Maintained by @AnOddDoorKnight", EditorStyles.boldLabel);
		EditorGUILayout.LabelField("B1NARY Debugger");
		EditorGUILayout.LabelField("Version 0.1.0b");
		if (iconImageForAbout == null)
			iconImageForAbout = Resources.Load<Texture2D>("img/UI/B1NARY");
		Vector2Int imageRatio = Ratio(iconImageForAbout.width,
			iconImageForAbout.height);
		float width = EditorGUIUtility.currentViewWidth - 16,
			height = width / imageRatio.x * imageRatio.y;
		GUI.DrawTexture(new Rect(8, Screen.height - (height * 1.3f), width, height), iconImageForAbout);
	}

	public override int Order => int.MinValue;


	// https://www.techtalk7.com/calculate-a-ratio-in-c/
	private static Vector2Int Ratio(int a, int b)
	{
		int gcd = GCD(a, b);
		return new Vector2Int(a / gcd, b / gcd);
	}
	private static int GCD(int a, int b) => b == 0 ? Math.Abs(a) : GCD(b, a % b);
}