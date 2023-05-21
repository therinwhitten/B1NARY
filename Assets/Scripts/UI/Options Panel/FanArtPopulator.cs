namespace B1NARY.UI
{
	using HideousDestructor.DataPersistence;
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.IO;
	using UnityEngine.UI;
	using UnityEngine;
	using B1NARY.UI.Saving;
	using Image = SixLabors.ImageSharp.Image;
	using SixLabors.ImageSharp;
	using System.Text;
	using TMPro;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Collections;
	using SixLabors.ImageSharp.Processing;
	using SixLabors.ImageSharp.Formats.Png;
	using System.Threading;
	using MEC;
	using UnityEngine.Rendering;

	public class FanArtPopulator : AutoPagePopulator
	{
		public GameObject Slot;
		public string subFolder = "Twitch TOS";
		[Space]
		public UnityEngine.UI.Image beegPanelImage;
		public TMP_Text textName;
		public TMP_Text textCredit;
		public GameObject fanArtPreview;
		public bool showHentai = false;

		public static List<FileInfo> GetAllImages(string subFolderName)
		{
			return RecursivelyGetFiles(SerializableSlot.StreamingAssets.GetOrCreateSubDirectory("Fanart")
				.GetOrCreateSubDirectory(subFolderName));
		}
		private static List<FileInfo> RecursivelyGetFiles(DirectoryInfo currentPath)
		{
			var output = new List<FileInfo>(currentPath.EnumerateFiles()
				.Where(path => path.Extension == ".png" || path.Extension == ".jpg"));
			IEnumerable<DirectoryInfo> directories = currentPath.EnumerateDirectories();
			if (directories.Any())
			{
				using (IEnumerator<DirectoryInfo> enumerator = directories.GetEnumerator())
					while (enumerator.MoveNext())
						output.AddRange(RecursivelyGetFiles(enumerator.Current));
			}
			return output;
		}

		protected virtual void Start()
		{
			base.Awake();
			StartCoroutine(ImageLoader());
			loadingThread = new Thread(() =>
			{
				while (!stop)
				{
					if (images.Count > 0)
					{
						(Action<byte[]> action, FileInfo info) = images.Dequeue();
						var memoryStream = new MemoryStream();
						using (FileStream stream = info.OpenRead())
							stream.CopyTo(memoryStream);
						action.Invoke(memoryStream.ToArray());
						memoryStream.Dispose();
					}
					else if (others.Count > 0)
					{
						others.Dequeue().Invoke();
					}
				}
			});
			loadingThread.Start();
		}
		private Thread loadingThread;
		private bool stop = false;
		private Queue<(Action<byte[]>, FileInfo)> images = new Queue<(Action<byte[]>, FileInfo)>();
		private Queue<Action> others = new Queue<Action>();

		// God, why the fuck is literally using ANYTHING WITH THREADS SO FUCKING HARD
		private IEnumerator ImageLoader()
		{
			List<FileInfo> list = GetAllImages(subFolder);
			for (int i = 0; i < list.Count; i++)
			{
				(bool hEnable, string creator, string name) = ParseName(list[i]);
				if (hEnable != showHentai)
					continue;

				FileInfo imageInfo = list[i];
				byte[] array = null;
				images.Enqueue((bytes => array = bytes, imageInfo));
				// Expensive stuff here
				while (array == null)
					yield return new WaitForEndOfFrame();
				Texture2D texture = ImageUtility.LoadImage(array);
				Sprite sprite = ImageUtility.CreateDefaultSprite(texture); 

				GameObject obj = AddEntry(Slot);
				LoadPanelBehaviour loadPanelBehaviour = obj.GetComponent<LoadPanelBehaviour>();
				loadPanelBehaviour.foregroundImage.sprite = sprite;
				loadPanelBehaviour.tmpText.text = name;

				loadPanelBehaviour.button.onClick.AddListener(() =>
				{
					Debug.Log("Clicked Button!");
					beegPanelImage.sprite = sprite;
					textName.text = $"Title: {name}";
					textCredit.text = $"Artist: {creator}";
					fanArtPreview.SetActive(true);
				});
			}
			stop = true;
		}

		public (bool hEnable, string creator, string name) ParseName(FileInfo file)
		{
			string fileName = file.NameWithoutExtension();
			// getting H-enable
			int hEnableIndex = fileName.LastIndexOf('_');
			bool hEnabled;
			if (hEnableIndex != -1)
			{
				string hEnableUnparsed = fileName.Substring(hEnableIndex + 1);
				if (hEnableUnparsed == "!henable")
					hEnabled = false;
				else if (hEnableUnparsed == "henable")
					hEnabled = true;
				else
					throw new InvalidCastException();
				fileName = fileName.Remove(hEnableIndex);
			}
			else
			{ 
				// Just to be secure
				hEnabled = true;
				Debug.LogWarning($"Filename '{fileName}' is found to have no hEnable in the name! Marking as hentai.");
			}

			// Getting Creator Name and File
			string[] splitFileName = fileName.Split(' ');
			HashSet<string> credit = new HashSet<string>() { "by", "credit" };

			string name;
			string creator = null;

			StringBuilder nameBuilder = new StringBuilder();

			for (int i = splitFileName.Length - 1; i >= 0; i--)
			{
				if (credit.Contains(splitFileName[i]) && creator == null)
				{
					creator = nameBuilder.Remove(nameBuilder.Length - 1, 1).ToString();
					nameBuilder = new StringBuilder();
					continue;
				}
				nameBuilder.Insert(0, $"{splitFileName[i]} ");
			}
			name = nameBuilder.Remove(nameBuilder.Length - 1, 1).ToString();

			return (hEnabled, creator, name);
		}
	}
}