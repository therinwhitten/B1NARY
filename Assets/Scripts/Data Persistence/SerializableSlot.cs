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

	[Serializable]
	public abstract class SerializableSlot : IDisposable, IDeserializationCallback
	{
		protected static T Deserialize<T>(FileInfo info) where T : SerializableSlot
		{
			if (!info.Exists)
				throw new Exception("BNY0001");
			using (var stream = new FileStream(info.FullName, FileMode.Open, FileAccess.Read))
				return (T)new BinaryFormatter().Deserialize(stream);
		}

		public static DirectoryInfo PersistentData { get; } = 
			new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData))
			.CreateSubdirectory(Application.productName);
		public static DirectoryInfo StreamingAssets { get; } = new DirectoryInfo(Application.streamingAssetsPath);

		#region Thumbnails
		public Texture2D ImageTexture
		{
			get => ImageUtility.LoadImage(image);
			set => image = ImageUtility.Compress(value.EncodeToJPG(), 512, 512);
		}
		public byte[] image;
		#endregion

		#region DateTime
		public DateTime LastSaved { get; private set; } = DateTime.Now;
		public TimeSpan TimeUsed { get; private set; } = TimeSpan.Zero;
		[NonSerialized]
		private Stopwatch stopwatch;
		#endregion

		public FileInfo fileInfo;
		public string Name
		{
			get => fileInfo.Name;
			set => fileInfo = fileInfo.Rename(value);
		}
		public SerializableSlot(FileInfo fileInfo)
		{
			this.fileInfo = fileInfo;
			stopwatch = Stopwatch.StartNew();
		}

		public virtual void Serialize()
		{
			LastSaved = DateTime.Now;
			TimeUsed += stopwatch.Elapsed;
			stopwatch = Stopwatch.StartNew();
			ImageTexture = ScreenCapture.CaptureScreenshotAsTexture();
			using (var stream = new FileStream(fileInfo.FullName, FileMode.Create, FileAccess.Write))
				new BinaryFormatter().Serialize(stream, this);
		}

		void IDisposable.Dispose()
		{
			Serialize();
		}

		void IDeserializationCallback.OnDeserialization(object sender)
		{
			stopwatch = Stopwatch.StartNew();
		}
	}
}