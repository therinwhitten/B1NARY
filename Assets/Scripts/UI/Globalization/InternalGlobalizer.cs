namespace B1NARY.Globalization
{
	using System;
	using System.Globalization;
	using UnityEngine;

	internal static class InternalGlobalizer
	{
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
		public static void Constructor()
		{
			CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
		}
	}
}