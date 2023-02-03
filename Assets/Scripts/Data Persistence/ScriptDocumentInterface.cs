namespace B1NARY.DataPersistence
{
	using System.Linq;
	using System.Collections;
	using System.Collections.Generic;
	using System;
	using B1NARY.Scripting;

	/// <summary>
	/// A delegate that is treated as an event, sending out the new value and 
	/// its source.
	/// </summary>
	public delegate void UpdatedConstantValue<T>(string key, T oldValue, T newValue, ScriptDocumentInterface.Collection<T> source);

	/// <summary>
	/// This is what the script writers interact with the game, beside setting
	/// the actual script themselves.
	/// </summary>
	[Serializable]
	public sealed class ScriptDocumentInterface
	{
		[Serializable]
		public sealed class Collection<T> : IDictionary<string, T>
		{
			/// <summary>
			/// Sends out an event that happens when a value is changed.
			/// </summary>
			public event UpdatedConstantValue<T> UpdatedValue;
			private readonly Dictionary<string, T> constants = new Dictionary<string, T>();
			private readonly Dictionary<string, Func<T>> pointers = new Dictionary<string, Func<T>>();
			public T this[string key]
			{
				get
				{
					if (constants.TryGetValue(key, out T value))
						return value;
					if (pointers.TryGetValue(key, out Func<T> dele))
						return dele.Invoke();
					throw new KeyNotFoundException(key);
				}
				set
				{
					if (pointers.TryGetValue(key, out Func<T> func))
					{
						T funcOldValue = func.Invoke();
						pointers.Remove(key);
						constants.Add(key, value);
						UpdatedValue?.Invoke(key, funcOldValue, constants[key], this);
						return;
					}
					constants.TryGetValue(key, out T oldValue);
					constants[key] = value;
					UpdatedValue?.Invoke(key, oldValue, constants[key], this);
				}
			}

			public ICollection<string> Keys => constants.Keys.Concat(pointers.Keys).ToArray();
			public ICollection<T> Values
			{
				get
				{
					List<T> whyCantIJustDoItForTheSameAsKeys = new List<T>(constants.Values);
					whyCantIJustDoItForTheSameAsKeys.AddRange(pointers.Values.Select(item => item.Invoke()));
					return whyCantIJustDoItForTheSameAsKeys;
				}
			}
			public int Count => constants.Count + pointers.Count;

			bool ICollection<KeyValuePair<string, T>>.IsReadOnly => false;

			public void Add(string key, T value)
			{
				if (pointers.ContainsKey(key))
					throw new InvalidOperationException(nameof(key));
				constants.Add(key, value);
			}

			public void Add(KeyValuePair<string, T> item)
			{
				if (pointers.ContainsKey(item.Key))
					throw new InvalidOperationException(nameof(item.Key));
				constants.Add(item.Key, item.Value);
			}

			public void Add(string key, Func<T> value)
			{
				if (constants.ContainsKey(key))
					throw new InvalidOperationException(nameof(key));
				pointers.Add(key, value);
			}


			public void Clear()
			{
				pointers.Clear();
				constants.Clear();
			}

			public bool Contains(KeyValuePair<string, T> item)
			{
				if (constants.TryGetValue(item.Key, out T value))
					if (value.Equals(item.Value))
						return true;
				return false;
			}

			public bool ContainsKey(string key)
			{
				return constants.ContainsKey(key) || pointers.ContainsKey(key);
			}

			void ICollection<KeyValuePair<string, T>>.CopyTo(KeyValuePair<string, T>[] array, int arrayIndex)
			{
				throw new NotImplementedException();
			}

			public IEnumerator<KeyValuePair<string, T>> GetEnumerator()
			{
				using (var enumerator = constants.AsEnumerable().GetEnumerator())
					while (enumerator.MoveNext())
						yield return enumerator.Current;
				using (var enumerator = pointers.AsEnumerable().GetEnumerator())
					while (enumerator.MoveNext())
						yield return new KeyValuePair<string, T>(enumerator.Current.Key, enumerator.Current.Value.Invoke());
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				using (var enumerator = constants.AsEnumerable().GetEnumerator())
					while (enumerator.MoveNext())
						yield return enumerator.Current;
				using (var enumerator = pointers.AsEnumerable().GetEnumerator())
					while (enumerator.MoveNext())
						yield return new KeyValuePair<string, T>(enumerator.Current.Key, enumerator.Current.Value.Invoke());
			}

			public bool Remove(string key)
			{
				if (constants.ContainsKey(key))
					return constants.Remove(key);
				if (pointers.ContainsKey(key))
					return pointers.Remove(key);
				return false;
			}

			bool ICollection<KeyValuePair<string, T>>.Remove(KeyValuePair<string, T> item)
			{
				throw new NotImplementedException();
			}

			public bool TryGetValue(string key, out T value)
			{
				if (constants.TryGetValue(key, out value))
					return true;
				if (pointers.TryGetValue(key, out Func<T> pointer))
				{
					value = pointer.Invoke();
					return true;
				}
				return false;
			}
		}

		public Collection<string> strings;
		public const string playerNameKey = "Player Name";
		public string PlayerName
		{
			get => strings[playerNameKey];
			set => strings[playerNameKey] = value;
		}
		public Collection<bool> bools;
		public Collection<int> ints;
		public Collection<float> floats;
		public Dictionary<int, ScriptLine> choice;

		public ScriptDocumentInterface()
		{
			strings = new Collection<string>();
			PlayerName = "MC";
			bools = new Collection<bool>()
			{
				["True"] = true,
				["False"] = false,
			};
			bools.Add("henable", HentaiEnabled);
			ints = new Collection<int>();
			floats = new Collection<float>();
			choice = new Dictionary<int, ScriptLine>();
			bool HentaiEnabled() => PlayerConfig.Instance.HentaiEnabled;
		}
	}
}