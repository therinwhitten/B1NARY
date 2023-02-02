namespace B1NARY.DataPersistence
{
	using System;
	using System.Drawing;
	using System.IO;
	using UnityEngine;
	using HideousDestructor.DataPersistence;

	/// <summary>
	/// A serializable image, typically for <see cref="SerializableSlot"/> to
	/// handle textures and images.
	/// </summary>
	[Serializable]
	public sealed class Thumbnail
	{
		/// <summary>
		/// The size of a typical 1:1 thumbnail, can be nicely seen by most monitors,
		/// including 2k ones.
		/// </summary>
		public const int typicalThumbnailSize = 512;

		/// <summary>
		/// Creates a new <see cref="Thumbnail"/> by using the player's current
		/// screen as a texture.
		/// </summary>
		/// <param name="maxWidth"> The maximum size of the width of the thumbnail. </param>
		/// <param name="maxHeight"> The maximum size of the height of the thumbnail. </param>
		/// <returns> The image of the screen. </returns>
		public static Thumbnail CreateWithScreenshot(int maxWidth = typicalThumbnailSize, int maxHeight = typicalThumbnailSize)
		{
			return new Thumbnail(new Vector2Int(maxWidth, maxHeight), ScreenCapture.CaptureScreenshotAsTexture());
		}

		private readonly byte[] data;
		/// <summary>
		/// The texture of the image. This is typically a JPG image if taken from
		/// a screenshot.
		/// </summary>
		public Texture2D Texture => ImageUtility.LoadImage(data);
		/// <summary>
		/// Retrieves the image from the byte array internally for the system to
		/// use.
		/// </summary>
		public Image SystemImage
		{
			get
			{
				using (var stream = new MemoryStream(data))
					return Image.FromStream(stream);
			}
		}

		public Thumbnail(Vector2Int thumbnailMaxSize, Texture2D texture)
			: this(thumbnailMaxSize, texture.EncodeToJPG())
		{
			
		}
		public Thumbnail(Vector2Int thumbnailMaxSize, byte[] image)
		{
			data = ImageUtility.Compress(image, thumbnailMaxSize.x, thumbnailMaxSize.y);
		}
	}
}