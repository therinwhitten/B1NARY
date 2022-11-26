namespace B1NARY.UI.Globalization
{
	using System;
	using TMPro;
	using UnityEngine;

	public class TextLanguageReflector : MonoBehaviour
	{
		public string Text
		{
			get
			{
				if (target == null)
					target = GetTarget();
				return target.Value;
			}
			set
			{
				if (target == null)
					target = GetTarget();
				target.Value = value;
			}
		}
		private Ref<string> target;

		public Ref<string> GetTarget()
		{
			Component[] components = gameObject.GetComponents(typeof(Component));
			for (int i = 0; i < components.Length; i++)
				if (components[i] is TMP_Text text)
				{
					target = new Ref<string>(() => text.text, set => text.text = set);
					return target;
				}
			throw new MissingComponentException("There is no types that is marked on the gameobject to modify!");
		}

		private string[] languageKeys, languageValues;

		private void Awake()
		{

		}
	}
	/*
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;
	using UnityEngine;
	using UnityEngine.UI;
	using B1NARY.DesignPatterns;
	using System.Runtime.Serialization.Formatters.Binary;
	using System.IO;

	[RequireComponent(typeof(Text)), AddComponentMenu("UI/Globalization/Text Font Language Changer")]
	public class FontLanguageChanger : Multiton<FontLanguageChanger>
	{
		private const string fontDataKey = "FontLanguageChangerData";
		private static FontData fontData;
		public static int LanguageIndex
		{
			get => fontData.languageIndex;
			set
			{
				fontData.languageIndex = value;
				fontData.Save(fontDataKey);
			}
		}
		public static IReadOnlyList<string> Languages => fontData.languages;

		static FontLanguageChanger()
		{
			if (FontData.TryLoad(fontDataKey, out var data))
				fontData = data.Value;
			else
				fontData = new FontData(0, new List<string>() { "english" });
			/*
			IEnumerator<FontLanguageChanger> enumerator = GetEnumerator();
			while (enumerator.MoveNext())
				for (int i = 0; i < languages.Count; i++)
					if (enumerator.Current.languageCollection.Contains(languages[i]))
						enumerator.Current.languageCollection.AddLanguage(languages[i]);
		}
		public static void AddLanguage(string language)
		{
			language = language.Trim().ToLower();
			if (Languages.Contains(language))
				throw new InvalidOperationException();
			fontData.languages.Add(language);
			fontData.Save(fontDataKey);
			IEnumerator<FontLanguageChanger> enumerator = GetEnumerator();
			while (enumerator.MoveNext())
				enumerator.Current.languageCollection.AddLanguage(language);
		}
		public static void RemoveLanguage(string language)
		{
			language = language.Trim().ToLower();
			fontData.languages.Remove(language);
			fontData.Save(fontDataKey);
			IEnumerator<FontLanguageChanger> enumerator = GetEnumerator();
			while (enumerator.MoveNext())
				enumerator.Current.languageCollection.RemoveLanguage(language);
		}

		private Text text;
		public LanguageCollection languageCollection = new LanguageCollection();
		protected override void MultitonAwake()
		{
			text = GetComponent<Text>();
		}
		private void Start()
		{
			text.text = languageCollection[LanguageIndex];
		}
	}

	

	[Serializable]
	internal struct FontData
	{
		private const string structPath = "/Language Font Data/";
		public static bool TryLoad(string fileName, out FontData? loadedData)
		{
			string path = Application.streamingAssetsPath + structPath + fileName;
			if (File.Exists(path))
			{
				using (var stream = new FileStream(path, FileMode.Open))
					loadedData = (FontData)new BinaryFormatter().Deserialize(stream);
				return true;
			}
			loadedData = null;
			return false;
		}

		public int languageIndex;
		public List<string> languages;
		public FontData(int index, List<string> languages)
		{
			languageIndex = index;
			this.languages = languages;
		}
		public void Save(string fileName)
		{
			using (var stream = new FileStream(Application.streamingAssetsPath + structPath + fileName, FileMode.Create))
				new BinaryFormatter().Serialize(stream, this);
		}
	}
	*/
}
