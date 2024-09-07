namespace B1NARY.UI
{
	using B1NARY.DataPersistence;
	using B1NARY.Globalization;
	using B1NARY.UI.Globalization;
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Text;
	using TMPro;
	using UnityEngine;
	using UnityEngine.UI;

	[DisallowMultipleComponent]
	public class FanartInspector : TextboxGlobalizer
	{
		public static (bool hEnable, string creator, string name) ParseName(string name)
		{
			// getting H-enable
			int hEnableIndex = name.LastIndexOf('_');
			bool hEnabled;
			if (hEnableIndex != -1)
			{
				string hEnableUnparsed = name.Substring(hEnableIndex + 1).ToLower();
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
			HashSet<string> credit = new() { "by", "By", "credit", "Credit" };

			string outName;
			string creator = null;

			StringBuilder nameBuilder = new();

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


		protected override void Reset()
		{
			UpdateLanguageList();
			if (languageValues.Count > 0)
				languageValues[0] = "Title:|Artist:";
		}

		protected override void OnEnable()
		{
			//if (fileName == null || authorName == null)
			//	throw new MissingFieldException();
			//PlayerConfig.Instance.language.AttachValue(UpdateLanguage);
		}
		protected override void OnDisable()
		{
			//PlayerConfig.Instance.language.ValueChanged -= UpdateLanguage;
		}
		protected override void UpdateLanguage(string newLanguage)
		{
			//string[] split = this[Languages.CurrentLanguage].Split('|');
			//fileName.text 
		}



		public TMP_Text fileName;
		public TMP_Text authorName;
		public Image image;

		public void InspectImage(Image image)
		{
			(_, string creator, string name) = ParseName(image.sprite.name);
			this.image.sprite = image.sprite;
			string unparsedPair = this[Languages.CurrentLanguage];
			string[] pairs = unparsedPair.Split('|');
			fileName.text = $"{pairs[0]} {name}";
			authorName.text = $"{pairs[1]} {creator}";
			gameObject.SetActive(true);
		}
	}
}
#if UNITY_EDITOR
namespace B1NARY.UI.Editor
{
	using B1NARY.Editor;
	using B1NARY.Globalization;
	using B1NARY.UI.Globalization;
	using System;
	using UnityEditor;
	using UnityEngine;

	[CustomEditor(typeof(FanartInspector))]
	public class FanartInspectorEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			FanartInspector inspector = (FanartInspector)target;
			inspector.UpdateLanguageList();
			inspector.fileName = DirtyAuto.Field(target, new("File Name"), inspector.fileName, true);
			inspector.authorName = DirtyAuto.Field(target, new("Author Name"), inspector.authorName, true);
			inspector.image = DirtyAuto.Field(target, new("Image"), inspector.image, true);
			EditorGUILayout.Space();
			for (int i = 0; i < inspector.languageKeys.Count; i++)
			{
				string[] split = inspector.languageValues[i].Split('|');
				if (split.Length != 2)
					split = new string[2];
				split[0] = DirtyAuto.Field(target, new(inspector.languageKeys[i]), split[0]);
				split[1] = DirtyAuto.Field(target, new(" "), split[1]);
				inspector.languageValues[i] = string.Join('|', split);
				EditorGUILayout.Space(1);
			}
			Languages.Instance.Editor_OnGUI();
		}
	}
}
#endif