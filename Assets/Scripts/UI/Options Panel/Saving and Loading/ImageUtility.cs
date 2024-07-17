namespace B1NARY
{
	using System;
	using System.IO;
	using UnityEngine;
	using SixLabors.ImageSharp;

	public static class ImageUtility
	{
		public static Texture2D LoadImage(byte[] image)
		{
			var texture = new Texture2D(8, 8, TextureFormat.RGBA32, false, false);
			texture.LoadImage(image);
			return texture;
		}
		// https://www.techtalk7.com/calculate-a-ratio-in-c/
		public static Vector2Int Ratio(int width, int height)
		{
			int gcd = GreatestCommonDivider(width, height);
			return new Vector2Int(width / gcd, height / gcd);
		}
		private static int GreatestCommonDivider(int a, int b) => 
			b == 0 ? Math.Abs(a) : GreatestCommonDivider(b, a % b);
		private static readonly Vector2 defaultAnchor = Vector2.one / 2f;
		public static Sprite CreateDefaultSprite(Texture2D texture2D)
		{
			return Sprite.Create(texture2D, new Rect(0f, 0f, texture2D.width, texture2D.height), defaultAnchor);
		}
	}
	
}