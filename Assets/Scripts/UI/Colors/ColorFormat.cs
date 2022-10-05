namespace B1NARY.UI
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using UnityEngine;
	using B1NARY.DataPersistence;


	// Have it change formats per scene if it exists.
	[Serializable]
	[CreateAssetMenu(fileName = "New UI Color Format", menuName = "B1NARY/Color UI Format", order = 1)]
	public class ColorFormat : ScriptableObject
	{
		public string primary = "FFFFFFFF", secondary = "FFFFFFFF";
		public List<string> keys = new List<string>(), colorHex = new List<string>();
		public IReadOnlyDictionary<string, string> ExtraUIValues
		{
			get
			{
				int i = -1;
				return colorHex.ToDictionary(hex => { i++; return keys[i]; });
			}
		}

		public void InsertKey(string old, string @new)
		{
			if (keys.Contains(@new))
				throw new InvalidOperationException();
			keys[keys.IndexOf(old)] = @new;
		}
		public void Append(string key, Color color)
		{
			keys.Add(key);
			colorHex.Add(ColorUtility.ToHtmlStringRGBA(color));
		}
		public void Modify(string key, in Color input)
		{
			int index = keys.IndexOf(key);
			if (input == null || colorHex[index] == ColorUtility.ToHtmlStringRGBA(input))
				return;
			colorHex[index] = ColorUtility.ToHtmlStringRGBA(input);
		}
		public void Remove(string key)
		{
			int index = keys.IndexOf(key);
			keys.RemoveAt(index);
			colorHex.RemoveAt(index);
		}

	}
}
