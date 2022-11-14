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
		/// <summary>
		/// Converts a byte to a float ranging from 0 to 1 into 0 to 255.
		/// </summary>
		public static float ToPercent(byte value)
		{
			return (float)value / byte.MaxValue;
		}
		/// <summary>
		/// Converts a float ranging from 0 to 1 into 0 to 255.
		/// </summary>
		public static byte ToByte(float percent)
		{
			return Convert.ToByte(percent * byte.MaxValue);
		}

		/// <summary>
		/// The primary color to be used by all UI. Defaulted to this color by
		/// default if something happens.
		/// </summary>
		public Color primaryUI = new Color(ToPercent(47), ToPercent(161), ToPercent(206));
		/// <summary>
		/// The secondary color to be used by all UI. The yang to the 
		/// <see cref="primaryUI"/> ying.
		/// </summary>
		public Color SecondaryUI = new Color(ToPercent(47), ToPercent(206), ToPercent(172));

		/// <summary>
		/// uses the setter/getter to modify values and notify the color format
		/// to change states.
		/// </summary>
		public IReadOnlyDictionary<string, Color> ExtraUIValues 
		{ 
			get => m_extraUIValues; set
			{
				savedKeys = value.Keys.ToArray();
				savedValues = value.Values.ToArray();
				m_extraUIValues = value;
			}
		}
		private IReadOnlyDictionary<string, Color> m_extraUIValues;

		/// <summary>
		/// The keys of the keyValuePair list. Separated due to the serialization 
		/// gods being angry at us for making our lives easier. Use 
		/// <see cref="SavedPairs"/> for the pairs.
		/// </summary>
		public string[] savedKeys = Array.Empty<string>();
		/// <summary>
		/// The values of the keyValuePair list. Separated due to the serialization 
		/// gods being angry at us for making our lives easier. Use 
		/// <see cref="SavedPairs"/> for the pairs.
		/// </summary>
		public Color[] savedValues = Array.Empty<Color>();
		/// <summary>
		/// The KeyValuePairs to modify. Its set as read only due to property 
		/// magic.
		/// </summary>
		public IReadOnlyList<KeyValuePair<string, Color>> SavedPairs
		{
			get => savedKeys.Zip(savedValues, (left, right) => new KeyValuePair<string, Color>(left, right)).ToList();
			set
			{
				savedKeys = new string[value.Count];
				savedValues = new Color[value.Count];
				for (int i = 0; i < value.Count; i++)
				{
					savedKeys[i] = value[i].Key;
					savedValues[i] = value[i].Value;
				}
				m_extraUIValues = SavedPairs.ToDictionary(pair => pair.Key, pair => pair.Value);
			}
		}

		/// <summary>
		/// On Deserialization/use.
		/// </summary>
		private void OnEnable()
		{
			m_extraUIValues = SavedPairs.ToDictionary(pair => pair.Key, pair => pair.Value);
		}
	}
}
