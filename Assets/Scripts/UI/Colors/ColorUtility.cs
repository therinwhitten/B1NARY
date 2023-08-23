namespace B1NARY.DataPersistence
{
	using System;
	using UnityEngine;
	using B1NARY.UI;
	using UnityColorUtility = UnityEngine.ColorUtility;

	public static class ColorUtility
	{
		public static Color Parse(string hexadecimal)
		{
			hexadecimal = hexadecimal.Trim();
			if (UnityColorUtility.TryParseHtmlString(hexadecimal, out Color color))
				return color;
			else
				throw new InvalidCastException(hexadecimal);
		}
		public static string ToRGB(this Color color)
		{
			return UnityColorUtility.ToHtmlStringRGB(color);
		}
		public static string ToRGBA(this Color color)
		{
			return UnityColorUtility.ToHtmlStringRGBA(color);
		}
	}
}