namespace B1NARY
{
	using UnityEngine;

	public class FrameLimiter
	{
		private void Awake()
		{
			Application.targetFrameRate = Screen.currentResolution.refreshRate;
		}
	}
}