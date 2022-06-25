using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FrameLimiter
{
	public static int Target
	{
		get => GamePreferences.GetInt("FrameLimitValue", 120);
		set => GamePreferences.SetInt("FrameLimitValue", value);
	}
	public static int VSyncCount
	{
		get => GamePreferences.GetInt("VSYNCFrameLimitCount", 1);
		set => GamePreferences.SetInt("VSYNCFrameLimitCount", value);
	}
	public static readonly GUIContent[] frameLimitSettings =
	{ 
		new GUIContent("Manual", "Put a manual frame limit cap here, no Auto Vsync."),
		new GUIContent("Full", "No Restriction, PC handles the frame cap."),
		new GUIContent("Half", "Restriction on half of frames the PC allows."),
		new GUIContent("Quarter", "Restriction on quarter of frames the PC allows."),
		new GUIContent("Eighth", "Restriction on eighth of the frames the PC allows.")
	};

	[ExecuteAlways]
	public static void ApplySettings()
	{
		QualitySettings.vSyncCount = VSyncCount;
		Application.targetFrameRate = Target;
	}
}
