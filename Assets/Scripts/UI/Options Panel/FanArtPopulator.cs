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

	public class FanArtPopulator : AutoPagePopulator
	{
		public GameObject Slot;
		public string subFolder = "Twitch TOS";

		public static IEnumerable<FileInfo> GetAllImages(string subFolderName)
		{
			FileInfo[] files = SerializableSlot.StreamingAssets.GetOrCreateSubDirectory("Fanart")
				.GetOrCreateSubDirectory(subFolderName).GetFiles();
			for (int i = 0; i < files.Length; i++)
			{
				if (files[i].Extension == ".png" || files[i].Extension == ".jpg")
					yield return files[i];
			}
		}

		private void OnEnable()
		{
			using (var enumerator = GetAllImages(subFolder).GetEnumerator())
				while (enumerator.MoveNext())
				{
					GameObject obj = AddEntry(Slot);
					LoadPanelBehaviour loadPanelBehaviour = obj.GetComponent<LoadPanelBehaviour>();
					loadPanelBehaviour.button.onClick.AddListener(() =>
					{
						Debug.Log("Clicked Button!");
					});
					FileInfo imageInfo = enumerator.Current;
					byte[] bytes; 
					using (Image image = Image.Load(imageInfo.FullName))
						using (var memoryStream = new MemoryStream())
						{
							image.SaveAsPng(memoryStream);
							bytes = memoryStream.ToArray();
						}
					Texture2D texture = ImageUtility.LoadImage(bytes);
					Sprite sprite = ImageUtility.CreateDefaultSprite(texture);
					loadPanelBehaviour.foregroundImage.sprite = sprite;
				}
		}
		private void OnDisable()
		{
			base.Clear();
		}
	}
}