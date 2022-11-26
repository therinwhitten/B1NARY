namespace B1NARY
{
	using UnityEngine;
// Want to get rid of VSync, and make Exclusive full screen the fullscreen option instead to enable freesync and gsync monitor technology.
//  Using the built in frame limit in this script, I want to attach to the resolution array in display options to restrict FPS based on refresh rate. 
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
}