namespace HideousDestructor.DataPersistence
{
	using System.IO;
	using System;
	using UnityEngine;
	using B1NARY;
	using System.Runtime.Serialization.Formatters.Binary;
	using UnityEditor.Graphs;
	using System.Drawing;
	using System.Diagnostics;
	using Debug = UnityEngine.Debug;
	using System.Runtime.Serialization;
	using System.Collections;

	/// <summary>
	/// A file for unity intended to be saved in <see cref="PersistentData"/>,
	/// using Binary Formatting.
	/// </summary>
	[Serializable]
	public abstract class SerializableSlot : IDisposable, IDeserializationCallback
	{
		/// <summary>
		/// Deserializes a 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="info"></param>
		/// <returns></returns>
		/// <exception cref="FileNotFoundException"></exception>
		protected static T Deserialize<T>(FileInfo info) where T : SerializableSlot
		{
			if (!info.Exists)
				throw new FileNotFoundException(info.FullName);
			using (var stream = new FileStream(info.FullName, FileMode.Open, FileAccess.Read))
			{
				object output = new BinaryFormatter().Deserialize(stream);
				if (output is T t)
					return t;
				throw new InvalidCastException($"'{info.Name}' is not a '{typeof(T).Name}'!");
			}
		}

		/// <summary>
		/// Gets the local appdata folder for saving settings, configs, etc.
		/// </summary>
		/// <remarks>
		/// Uses window's <see cref="Environment.GetFolderPath(Environment.SpecialFolder)"/> 
		/// structure due to <see cref="Application.persistentDataPath"/>
		/// being incredibly buggy, especially when the folder is missing 
		/// from the appdata; causing it to not display any directory location 
		/// at all.
		/// </remarks>
		public static DirectoryInfo PersistentData { get; } =
			new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData))
			.CreateSubdirectory(Application.productName);
		/// <summary>
		/// Gets the streaming assets folder with a <see cref="DirectoryInfo"/>.
		/// </summary>
		public static DirectoryInfo StreamingAssets { get; } = new DirectoryInfo(Application.streamingAssetsPath);

		#region Thumbnails
		/// <summary>
		/// Converts the base of <see cref="image"/> to a texture for Unity.
		/// Compressed.
		/// </summary>
		public Texture2D ImageTexture
		{
			get => ImageUtility.LoadImage(image);
			set => image = ImageUtility.Compress(value.EncodeToJPG(), 512, 512);
		}
		/// <summary>
		/// an array of bytes, saved as an image.
		/// </summary>
		public byte[] image;
		#endregion

		#region DateTime
		/// <summary>
		/// When the <see cref="Serialize"/> command is last used.
		/// </summary>
		public DateTime LastSaved { get; private set; } = DateTime.Now;
		/// <summary>
		/// How long the save slot was used in a session.
		/// </summary>
		public TimeSpan TimeUsed { get; private set; } = TimeSpan.Zero;
		[NonSerialized]
		private Stopwatch stopwatch;
		#endregion

		protected internal FileInfo fileInfo;

		public SerializableSlot(FileInfo fileInfo)
		{
			this.fileInfo = fileInfo;
			OnDeserialization(this);
		}

		public virtual void Serialize()
		{
			LastSaved = DateTime.Now;
			TimeUsed += stopwatch is null ? TimeSpan.Zero : stopwatch.Elapsed;
			stopwatch?.Stop();
			stopwatch = Stopwatch.StartNew();
			try
			{
				ImageTexture = ScreenCapture.CaptureScreenshotAsTexture();
			}
			catch (Exception ex)
			{
				Debug.LogException(new Exception("This may have to do how " +
					"quickly the computer handles frames. Starting coroutine to save..", ex));
				UnityEngine.Object.FindObjectOfType<MonoBehaviour>().StartCoroutine(Await());
				return;
				IEnumerator Await()
				{
					yield return new WaitForEndOfFrame();
					ImageTexture = ScreenCapture.CaptureScreenshotAsTexture();
					using (var stream = fileInfo.Open(FileMode.Create, FileAccess.Write))
						new BinaryFormatter().Serialize(stream, this);
					fileInfo.Refresh();
				}
			}
			using (var stream = fileInfo.Open(FileMode.Create, FileAccess.Write))
				new BinaryFormatter().Serialize(stream, this);
			fileInfo.Refresh();
		}

		void IDisposable.Dispose()
		{
			Serialize();
		}

		public virtual void OnDeserialization(object sender)
		{
			stopwatch = Stopwatch.StartNew();
		}
	}
}