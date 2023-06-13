﻿namespace B1NARY
{
	using System;
	using UnityEngine;

	public class FPSHandler
	{
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
		private static void Constructor()
		{
			PlayerConfig.Instance.graphics.frameRate.AttachValue(count => Application.targetFrameRate = count);
		}

		public void ChangeTarget(int frameRate)
		{
			PlayerConfig.Instance.graphics.frameRate.Value = frameRate;
		}
	}
}