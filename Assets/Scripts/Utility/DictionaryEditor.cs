#if UNITY_EDITOR
namespace B1NARY.Editor
{
	using UnityEditor;
	using UnityEngine;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Collections;
	using B1NARY.DataPersistence;

	/// <summary>
	/// A key-value pair editor primarily used for <see cref="ScriptableObject"/>s
	/// </summary>
	public class DictionaryEditor<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
	{
		private static object Modify(object oldValue, Rect rect) 
		{
			// Would use better tools, but the bullshit primitive ass Live2D 
			// - Cubism Legacy code tells me no. 
			if (oldValue is null)
				throw new ArgumentNullException();
			if (oldValue is UnityEngine.Object @object)
				return EditorGUI.ObjectField(rect, @object, @object.GetType(), false);
			else if (oldValue is GUIContent content)
			{
				EditorGUI.LabelField(rect, content);
				return null;
			}
			else if (oldValue is string @string)
				return EditorGUI.TextField(rect, @string);
			else if (oldValue is float @float)
				return EditorGUI.FloatField(rect, @float);
			else if (oldValue is int @int)
				return EditorGUI.IntField(rect, @int);
			else if (oldValue is Color color)
				return EditorGUI.ColorField(rect, color);
			throw new InvalidCastException(oldValue.GetType().Name);
		}


		/// <summary>
		/// Source material of the array/list.
		/// </summary>
		public IReadOnlyList<KeyValuePair<TKey, TValue>> Items => m_items;
		public TKey defaultKey = default;
		public TValue defaultValue = default;
		private List<KeyValuePair<TKey, TValue>> m_items;
		/// <summary>
		/// Creates an instance using the source dictionary. Roughly similar to
		/// <see cref="Dictionary{TKey, TValue}"/> to <see cref="IEnumerable{KeyValuePair{TKey, TValue}}"/>
		/// </summary>
		/// <param name="values"> The dictionary to base off of. </param>
		public DictionaryEditor(Dictionary<TKey, TValue> values) : this(values.AsEnumerable())
		{
			
		}
		/// <summary>
		/// Uses the same input from the list of keyValuePairs to modify.
		/// </summary>
		public DictionaryEditor(List<KeyValuePair<TKey, TValue>> values)
		{
			m_items = values;
		}
		/// <summary>
		/// Creates an instance using the enumerable pairKeys.
		/// </summary>
		public DictionaryEditor(IEnumerable<KeyValuePair<TKey, TValue>> values)
		{
			m_items = new List<KeyValuePair<TKey, TValue>>(values);
		}
		/// <summary>
		/// Creates a new instance with no data.
		/// </summary>
		public DictionaryEditor()
		{
			m_items = new List<KeyValuePair<TKey, TValue>>();
		}

		/// <summary>
		/// Repaints the array/list with it's own system.
		/// </summary>
		/// <returns> If the object is dirty. </returns>
		public bool Repaint()
		{
			bool dirty = false;
			if (GUILayout.Button($"Add New Entry {{{typeof(TKey).Name}, {typeof(TValue).Name}}}"))
			{
				m_items.Add(new KeyValuePair<TKey, TValue>(defaultKey, defaultValue));
				dirty = true;
			}
			for (int i = 0; i < m_items.Count; i++)
			{
				// Rects & Positions
				Rect fullRect = EditorGUI.IndentedRect(GUILayoutUtility.GetRect(Screen.width, 22f));
				fullRect.yMax -= 1f;
				fullRect.yMin += 1f;
				Rect keyRect = new(fullRect)
				{
					xMax = (fullRect.xMax / 3f) + 24f,
				},
				removeButtonRect = new(fullRect)
				{
					width = fullRect.width / 5f,
					x = fullRect.xMax - (fullRect.width / 5f),
				},
				valueRect = new(fullRect)
				{
					xMin = keyRect.xMax - 14f,
					xMax = removeButtonRect.xMin - 2f,
				};

				// Modifying
				TKey newData = (TKey)Modify(m_items[i].Key, keyRect);
				if (!newData.Equals(m_items[i].Key))
				{
					ModifyKey(i, newData);
					dirty = true;
				}
				TValue newValue = (TValue)Modify(m_items[i].Value, valueRect); 
				if (!newValue.Equals(m_items[i].Value))
				{
					ModifyValue(i, newValue);
					dirty = true;
				}
				if (GUI.Button(removeButtonRect, "Remove"))
				{
					RemoveAt(i);
					dirty = true;
				}
			}
			return dirty;
		}

		/// <summary>
		/// Gets or Sets The <see cref="TValue"/> via the index, as it is in 
		/// a list.
		/// </summary>
		/// <param name="index"> The index for the list to access to. </param>
		/// <returns> The <see cref="TValue"/>. </returns>
		public TValue this[int index]
		{
			get => m_items[index].Value;
			set => m_items[index] = new KeyValuePair<TKey, TValue>(m_items[index].Key, value);
		}

		/// <summary>
		/// Modifies 
		/// </summary>
		/// <param name="oldKey"></param>
		/// <param name="newKey"></param>
		/// <exception cref="IndexOutOfRangeException"></exception>
		public void ModifyKey(TKey oldKey, TKey newKey, bool warnOnModifyMultiple = true)
		{
			for (int elementAt = 0; elementAt < m_items.Count; elementAt++)
			{
				if (!m_items[elementAt].Key.Equals(oldKey))
					continue;
				ModifyKey(elementAt, newKey);
				break;
			}
			throw new IndexOutOfRangeException(oldKey.ToString());
		}
		public void ModifyKey(int index, TKey newKey)
		{
			m_items[index] = new KeyValuePair<TKey, TValue>(newKey, m_items[index].Value); 
		}

		public void ModifyValue(TValue oldValue, TValue newValue, bool warnOnModifyMultiple = true)
		{
			int countModified = 0;
			for (int i = 0; i < m_items.Count; i++)
			{
				if (!oldValue.Equals(m_items[i].Value))
					continue;
				countModified++;
				ModifyValue(i, newValue);
			}
			if (countModified <= 0)
				throw new IndexOutOfRangeException(oldValue.ToString());
			if (warnOnModifyMultiple && countModified > 1)
				Debug.LogWarning($"Should not change a value that matches other values!");
		}
		public void ModifyValue(int index, TValue newValue) => this[index] = newValue;

		public void Add(TKey key, TValue value) => Add(new KeyValuePair<TKey, TValue>(key, value));
		public void Add(KeyValuePair<TKey, TValue> pair)
		{
			for (int i = 0; i < m_items.Count; i++)
				if (pair.Key.Equals(m_items[i]))
					throw new Exception($"{pair.Key} and {m_items[i].Key} are the same value in the KeyPairs!");
			m_items.Add(pair);
		}

		public void RemoveAt(int index)
		{
			m_items.RemoveAt(index);
		}

		public Dictionary<TKey, TValue> Flush()
		{
			return m_items.ToDictionary(pair => pair.Key, pair => pair.Value);
		}

		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
		{
			return ((IEnumerable<KeyValuePair<TKey, TValue>>)m_items).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable)m_items).GetEnumerator();
		}
	}
}
#endif