namespace B1NARY.UI
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using UnityEngine;
	using B1NARY.UI.Saving;
	using TMPro;
	using System.Linq;
	using System.Collections;
	using System.Threading;
	using B1NARY.DataPersistence;
	using B1NARY.IO;

	[DisallowMultipleComponent]
	public class FanArtPopulator : AutoPagePopulator
	{
		public GameObject Slot;
		public string subFolder = "Twitch TOS";
		[Space]
		public UnityEngine.UI.Image beegPanelImage;
		public TMP_Text textName;
		public TMP_Text textCredit;
		public GameObject fanArtPreview;

		public static List<OSFile> GetAllImages(string subFolderName)
		{
			return RecursivelyGetFiles(SaveSlot.StreamingAssets.GetSubdirectories("Fanart", subFolderName));
		}
		private static List<OSFile> RecursivelyGetFiles(OSDirectory currentPath)
		{
			List<OSFile> output = new(currentPath.EnumerateFiles()
				.Where(path => path.Extension == ".png" || path.Extension == ".jpg"));
			OSDirectory[] directories = currentPath.GetDirectories();
			for (int i = 0; i < directories.Length; i++)
				output.AddRange(RecursivelyGetFiles(directories[i]));
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
						ImageReader info = images.Dequeue();
						var memoryStream = new MemoryStream();
						using FileStream stream = info.FileInfo.OpenRead();
						stream.CopyTo(memoryStream);
						info.Action.Invoke(memoryStream.ToArray());
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
		private record ImageReader(Action<byte[]> Action, OSFile FileInfo);
		private readonly Queue<ImageReader> images = new();
		private readonly Queue<Action> others = new();

		// God, why the fuck is literally using ANYTHING WITH THREADS SO FUCKING HARD
		private IEnumerator ImageLoader()
		{
			List<OSFile> list = GetAllImages(subFolder);
			for (int i = 0; i < list.Count; i++)
			{
				(bool hEnable, string creator, string name) = FanartInspector.ParseName(list[i].NameWithoutExtension);
				if (hEnable && PlayerConfig.Instance.hEnable.Value == false)
					continue;

				OSFile imageInfo = list[i];
				byte[] array = null;
				images.Enqueue(new ImageReader(bytes => array = bytes, imageInfo));
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

		
	}
}