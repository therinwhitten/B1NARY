namespace B1NARY
{
	using B1NARY.DataPersistence;
	using System;
	using UnityEngine;

	public class FPSHandler
	{
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
		private static void Constructor()
		{
			PlayerConfig.Instance.graphics.refreshRate.AttachValue(count => Application.targetFrameRate = count);
		}

		public void ChangeTarget(int frameRate)
		{
			PlayerConfig.Instance.graphics.refreshRate.Value = frameRate;
		}
	}
}