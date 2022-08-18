namespace B1NARY.UI.Globalization
{
	using System;
	using System.Collections.Generic;
	using System.Collections;

	[Serializable]
	public class LanguageCollection : IEnumerable<KeyValuePair<string, string>>
	{
		/// <summary>
		/// Messing with these values are not recommend and will cause synchronisation
		/// issues! These are only exposed to please the unity gods.
		/// </summary>
		public List<string> keys, values;

		public LanguageCollection()
		{
			keys = new List<string>();
			values = new List<string>();
		}
		public string this[string key]
		{
			get => values[keys.IndexOf(key)];
			set => values[keys.IndexOf(key)] = value;
		}
		public string this[int index]
		{
			get => values[index];
			set => values[index] = value;
		}
		internal void AddLanguage(string key)
		{
			keys.Add(key.Trim().ToLower());
			values.Add(string.Empty);
		}
		internal void RemoveLanguage(string key)
		{
			key = key.Trim().ToLower();
			int index = key.IndexOf(key);
			keys.RemoveAt(index);
			values.RemoveAt(index);
		}
		public bool Contains(string key) => keys.Contains(key);

		IEnumerator<KeyValuePair<string, string>> IEnumerable<KeyValuePair<string, string>>.GetEnumerator()
		{
			for (int i = 0; i < keys.Count; i++)
				yield return new KeyValuePair<string, string>(keys[i], values[i]);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			for (int i = 0; i < keys.Count; i++)
				yield return new KeyValuePair<string, string>(keys[i], values[i]);
		}
	}
}
