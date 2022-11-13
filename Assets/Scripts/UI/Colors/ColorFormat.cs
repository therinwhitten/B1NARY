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
	public class ColorFormat : ScriptableObject, ISerializationCallbackReceiver
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


		public IReadOnlyDictionary<string, Color> ExtraUIValues 
		{ 
			get => m_extraUIValues; set
			{
				value.Select(pair => (pair.Key, ColorUtility.ToHtmlStringRGBA(pair.Value)));
				m_extraUIValues = (Dictionary<string, Color>)value;
			}
		}
		/// <summary>
		/// The keys of the keyValuePair list. Separated due to the serialization 
		/// gods being angry at us for making our lives easier. Use 
		/// <see cref="SavedPairs"/> for the pairs.
		/// </summary>
		public List<string> savedKeys = new List<string>();
		/// <summary>
		/// The values of the keyValuePair list. Separated due to the serialization 
		/// gods being angry at us for making our lives easier. Use 
		/// <see cref="SavedPairs"/> for the pairs.
		/// </summary>
		public List<string> savedValues = new List<string>();
		/// <summary>
		/// The KeyValuePairs to modify. Its set as read only due to property 
		/// magic.
		/// </summary>
		public IReadOnlyList<KeyValuePair<string, string>> SavedPairs
		{
			get => savedKeys.Zip(savedValues, (left, right) => new KeyValuePair<string, string>(left, right)).ToList();
			set
			{
				savedKeys.Clear();
				savedValues.Clear();
				for (int i = 0; i < value.Count; i++)
				{
					savedKeys.Add(value[i].Key);
					savedValues.Add(value[i].Value);
				}
			}
		}
		private Dictionary<string, Color> m_extraUIValues = new Dictionary<string, Color>();


		void ISerializationCallbackReceiver.OnBeforeSerialize()
		{
			SavedPairs = m_extraUIValues.Select(pair => new KeyValuePair<string, string>(pair.Key, ColorUtility.ToHtmlStringRGBA(pair.Value))).ToList();
		}
		/// <summary>
		/// On Deserialization/use.
		/// </summary>
		private void OnEnable()
		{
			m_extraUIValues = SavedPairs.ToDictionary(pair => pair.Key, pair => { ColorUtility.TryParseHtmlString(pair.Value, out Color color); return color; });
		}

		void ISerializationCallbackReceiver.OnAfterDeserialize()
		{
			
		}
	}
}
