namespace B1NARY.UI
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Text;
	using TMPro;
	using UnityEngine;
	using UnityEngine.UI;

	public class FanartInspector : MonoBehaviour
	{
		public static (bool hEnable, string creator, string name) ParseName(string name)
		{
			// getting H-enable
			int hEnableIndex = name.LastIndexOf('_');
			bool hEnabled;
			if (hEnableIndex != -1)
			{
				string hEnableUnparsed = name.Substring(hEnableIndex + 1);
				if (hEnableUnparsed == "!henable")
					hEnabled = false;
				else if (hEnableUnparsed == "henable")
					hEnabled = true;
				else
					throw new InvalidCastException();
				name = name.Remove(hEnableIndex);
			}
			else
			{
				// Just to be secure
				hEnabled = true;
				Debug.LogWarning($"Filename '{name}' is found to have no hEnable in the name! Marking as hentai.");
			}

			// Getting Creator Name and File
			string[] splitFileName = name.Split(' ');
			HashSet<string> credit = new HashSet<string>() { "by", "credit" };

			string outName;
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
			outName = nameBuilder.Remove(nameBuilder.Length - 1, 1).ToString();

			return (hEnabled, creator, outName);
		}




		public TMP_Text fileName;
		public TMP_Text authorName;
		public Image image;

		public void InspectImage(Image image)
		{
			(_, string creator, string name) = ParseName(image.sprite.name);
			authorName.text = creator;
			fileName.text = name;
			this.image.sprite = image.sprite;
		}
	}
}