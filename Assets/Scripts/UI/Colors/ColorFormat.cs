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
		public static float ToPercent(byte value)
		{
			return (float)value / byte.MaxValue;
		}

		public static byte ToByte(float percent)
		{
			return Convert.ToByte(percent * byte.MaxValue);
		}

		public Color primaryUI = new Color(ToPercent(47), ToPercent(161), ToPercent(206)),
			SecondaryUI = new Color(ToPercent(47), ToPercent(206), ToPercent(172));
		public List<string> keys = new List<string>();
		public List<ColorSerializable> values = new List<ColorSerializable>();


		public IReadOnlyDictionary<string, Color> ExtraUIValues => Pairs
			.ToDictionary(pair => pair.Key, pair => pair.Value);
		public IEnumerable<KeyValuePair<string, Color>> Pairs =>
			keys.Zip(values, (key, value) => new KeyValuePair<string, Color>(key, (Color)value));

		public void InsertKey(string old, string @new)
		{
			if (keys.Contains(@new))
				throw new InvalidOperationException();
			keys[keys.IndexOf(old)] = @new;
		}
		public void Append(string key, Color color)
		{
			keys.Add(key);
			values.Add((ColorSerializable)color);
		}
		public void Modify(string key, in Color input)
		{
			int index = keys.IndexOf(key);
			if (input == null || (Color)values[index] == input)
				return;
			values.RemoveAt(index);
			values.Add((ColorSerializable)input);
		}
		public void Remove(string key)
		{
			int index = keys.IndexOf(key);
			keys.RemoveAt(index);
			values.RemoveAt(index);
		}

	}
}
