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
		private PngEncoder encoder = new PngEncoder()
		{
			CompressionLevel = PngCompressionLevel.NoCompression,
		};

		[Space]
		public UnityEngine.UI.Image beegPanelImage;
		public TMP_Text textName;
		public TMP_Text textCredit;
		public GameObject fanArtPreview;

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

		private void OnEnable()
		{
			StartCoroutine(LOADFUCKINGIMAGESWHYISITSOHARDUNITY());
		}
		// God, why the fuck is literally using ANYTHING WITH THREADS 
		private IEnumerator LOADFUCKINGIMAGESWHYISITSOHARDUNITY()
		{
			List<FileInfo> list = GetAllImages(subFolder);
			for (int i = 0; i < list.Count; i++)
			{
				(bool hEnable, string creator, string name) = ParseName(list[i]);
				bool final = i - 1 == list.Count;
				if (hEnable && PlayerConfig.Instance.hEnable == false)
					continue;

				FileInfo imageInfo = list[i];
				// Expensive stuff here
				Task<MemoryStream> spriteTask = Task.Run(() => LoadImage(imageInfo, i));
				while (!spriteTask.GetAwaiter().IsCompleted)
					yield return new WaitForEndOfFrame();
				Sprite sprite;
				using (MemoryStream stream = spriteTask.Result)
					sprite = ImageUtility.CreateDefaultSprite(ImageUtility.LoadImage(stream.ToArray())); 

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


				async Task<MemoryStream> LoadImage(FileInfo file, int index)
				{
					var memoryStream = new MemoryStream();
					using (Image image = Image.Load(file.FullName))
						await image.SaveAsPngAsync(memoryStream, encoder);
					return memoryStream;
				}
			}
		}
		private void OnDisable()
		{
			base.Clear();
		}
		Thread imageLoader;


		private async Task LoadAllImages()
		{
			List<FileInfo> list = GetAllImages(subFolder);
			List<Task<Sprite>> tasks = new List<Task<Sprite>>(list.Count);
			for (int i = 0; i < list.Count; i++)
			{
				if (!enabled)
					return;
				(bool hEnable, string creator, string name) = ParseName(list[i]);
				if (hEnable && PlayerConfig.Instance.hEnable == false)
					continue;

				GameObject obj = AddEntry(Slot);
				LoadPanelBehaviour loadPanelBehaviour = obj.GetComponent<LoadPanelBehaviour>();

				loadPanelBehaviour.tmpText.text = name;
				FileInfo imageInfo = list[i];
				// Expensive stuff here
				Task<Sprite> task = LoadImage(imageInfo, i);
				tasks.Add(task);
				
				
				loadPanelBehaviour.button.onClick.AddListener(() =>
				{
					Debug.Log("Clicked Button!");
					beegPanelImage.sprite = task.Result;
					textName.text = $"Title: {name}";
					textCredit.text = $"Artist: {creator}";
					fanArtPreview.SetActive(true);
				});


				async Task<Sprite> LoadImage(FileInfo file, int index)
				{
					byte[] bytes;
					using (Image image = await Image.LoadAsync(file.FullName))
					using (var memoryStream = new MemoryStream())
					{
						await image.SaveAsPngAsync(memoryStream, encoder);
						bytes = memoryStream.ToArray();
					}
					Texture2D texture = ImageUtility.LoadImage(bytes);
					Sprite sprite = ImageUtility.CreateDefaultSprite(texture);
					loadPanelBehaviour.foregroundImage.sprite = sprite;
					return sprite;
				}
			}
			await Task.WhenAll(tasks);
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