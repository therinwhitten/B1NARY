namespace B1NARY
{
	using System;
	using UnityEngine;

	public static class ImageUtility
	{
		// https://www.techtalk7.com/calculate-a-ratio-in-c/
		public static Vector2Int Ratio(int width, int height)
		{
			int gcd = GreatestCommonDivider(width, height);
			return new Vector2Int(width / gcd, height / gcd);
		}
		private static int GreatestCommonDivider(int a, int b) => 
			b == 0 ? Math.Abs(a) : GreatestCommonDivider(b, a % b);
	}
}