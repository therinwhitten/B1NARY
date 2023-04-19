namespace B1NARY.Globalization
{
	using System;
	using System.Globalization;
	using UnityEngine;

	internal static class InternalGlobalizer
	{
		[RuntimeInitializeOnLoadMethod]
		public static void Constructor()
		{
			CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
		}
	}
}