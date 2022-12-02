namespace B1NARY
{
	using UnityEngine;

	public class FrameLimiter
	{
		void Start() 
		{
			Application.targetFramerate = Screen.currentResolution.refreshRate;
		}
		
	}
}