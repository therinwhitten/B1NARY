namespace B1NARY
{
	using System;
	using System.Drawing;
	using System.IO;
	using UnityEngine;

	public static class ImageUtility
	{
		public static byte[] Compress(byte[] source, int maxWidth, int maxHeight)
		{
			if (source is null || source.Length <= 0)
				return Array.Empty<byte>();
			using (var stream = new MemoryStream(source))
			{
				Bitmap bitmap = new Bitmap(stream);
				Vector2Int imageRatio = Ratio(bitmap.Width, bitmap.Height);
				var imageSize = new Vector2Int(bitmap.Width, bitmap.Height);
				// making width to 512, and height as follows
				if (imageSize.x > maxWidth)
				{
					imageSize.y = (int)((float)maxWidth / imageRatio.x * imageRatio.y);
					imageSize.x = maxWidth;
				}
				if (imageSize.y > maxHeight)
				{
					imageRatio = ImageUtility.Ratio(imageSize.x, imageSize.y);
					imageSize.x = (int)((float)maxHeight / imageRatio.y * imageRatio.x);
					imageSize.y = maxHeight;
				}
				Image subImage = bitmap.GetThumbnailImage(imageSize.x, imageSize.y, null, IntPtr.Zero);
				bitmap.Dispose();
				return (byte[])new ImageConverter().ConvertTo(subImage, typeof(byte[]));
			}
		}
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
	}
}