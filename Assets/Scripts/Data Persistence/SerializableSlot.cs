namespace HideousDestructor.DataPersistence
{
	using System.IO;
	using System;
	using UnityEngine;
	using System.Runtime.Serialization.Formatters.Binary;
	using System.Diagnostics;
	using Debug = UnityEngine.Debug;
	using System.Runtime.Serialization;
	using System.Collections;
	using B1NARY.DataPersistence;

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
		public static DirectoryInfo PersistentData { get; } = ((Func<DirectoryInfo>)(() => 
		{
			return new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData))
				.CreateSubdirectory(Application.productName.Trim());
		})).Invoke();
			
		/// <summary>
		/// Gets the streaming assets folder with a <see cref="DirectoryInfo"/>.
		/// </summary>
		public static DirectoryInfo StreamingAssets { get; } = new DirectoryInfo(Application.streamingAssetsPath);

		#region Thumbnail
		/// <summary>
		/// The thumbnail of the save slot. Dependent on <see cref="UsesThumbnail"/>.
		/// </summary>
		public Thumbnail Thumbnail { get; private set; }
		/// <summary>
		/// On serialization, you can save the thumbnail. Enabled by default.
		/// </summary>
		public virtual bool UsesThumbnail { get; } = true;
		#endregion

		#region DateTime
		/// <summary>
		/// When the <see cref="Serialize"/> command is last used.
		/// </summary>
		public DateTime LastSaved { get; protected set; } = DateTime.Now;
		/// <summary>
		/// How long the save slot was used in a session.
		/// </summary>
		public TimeSpan TimeUsed { get; protected set; } = TimeSpan.Zero;
		[NonSerialized]
		protected Stopwatch stopwatch;
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
			if (UsesThumbnail)
				try
				{
					Thumbnail = Thumbnail.CreateWithScreenshot();
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
						Thumbnail = Thumbnail.CreateWithScreenshot();
						using (var stream = fileInfo.Open(FileMode.Create, FileAccess.Write))
							new BinaryFormatter().Serialize(stream, this);
						fileInfo.Refresh();
					}
				}
			else
				Thumbnail = null;
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