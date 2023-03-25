namespace B1NARY.DataPersistence
{
	using System.Linq;
	using System.Collections;
	using System.Collections.Generic;
	using System;
	using B1NARY.Scripting;
	using System.Runtime.Serialization;
	using System.Xml.Serialization;

	/// <summary>
	/// A delegate that is treated as an event, sending out the new value and 
	/// its source.
	/// </summary>
	public delegate void UpdatedConstantValue<T>(string key, T oldValue, T newValue, Collection<T> source);

	[Serializable]
	public sealed class Collection<T> : IDictionary<string, T>
	{
		/// <summary>
		/// Sends out an event that happens when a value is changed.
		/// </summary>
		[field: NonSerialized, XmlIgnore]
		public event UpdatedConstantValue<T> UpdatedValue;
		private Dictionary<string, T> m_constants = new Dictionary<string, T>();
		public Dictionary<string, T> Constants
		{
			get
			{
				if (m_constants is null)
					return m_constants = new Dictionary<string, T>();
				return m_constants;
			}
		}
		[field: NonSerialized, XmlIgnore]
		private Dictionary<string, Func<T>> m_pointers = new Dictionary<string, Func<T>>();
		public Dictionary<string, Func<T>> Pointers
		{
			get
			{
				if (m_pointers is null)
					return m_pointers = new Dictionary<string, Func<T>>();
				return m_pointers;
			}
		}

		public T this[string key]
		{
			get
			{
				if (Constants.TryGetValue(key, out T value))
					return value;
				if (Pointers.TryGetValue(key, out Func<T> dele))
					return dele.Invoke();
				throw new KeyNotFoundException(key);
			}
			set
			{
				if (Pointers.TryGetValue(key, out Func<T> func))
				{
					T funcOldValue = func.Invoke();
					Pointers.Remove(key);
					Constants.Add(key, value);
					UpdatedValue?.Invoke(key, funcOldValue, Constants[key], this);
					return;
				}
				Constants.TryGetValue(key, out T oldValue);
				Constants[key] = value;
				UpdatedValue?.Invoke(key, oldValue, Constants[key], this);
			}
		}

		/// <summary>
		/// Determines if the key has a point or not.
		/// </summary>
		/// <returns> 
		/// <see langword="true"/> if it is a pointer, <see langword="false"/> 
		/// if its a constant, <see langword="null"/> if it is not located 
		/// in the collection.
		/// </returns>
		public bool? IsPointer(string key)
		{
			if (Constants.ContainsKey(key))
				return false;
			if (Pointers.ContainsKey(key))
				return true;
			return null;
		}

		public ICollection<string> Keys
		{
			get
			{
				List<string> keys = new List<string>(Constants.Keys);
				keys.AddRange(Pointers.Keys);
				return keys;
			}
		}

		public ICollection<T> Values
		{
			get
			{
				List<T> values = new List<T>(Constants.Values);
				values.AddRange(Pointers.Values.Select(item => item.Invoke()));
				return values;
			}
		}
		public int Count => Constants.Count + Pointers.Count;

		bool ICollection<KeyValuePair<string, T>>.IsReadOnly => false;

		public void Add(string key, T value)
		{
			if (Pointers.ContainsKey(key))
				throw new InvalidOperationException(nameof(key));
			Constants.Add(key, value);
		}

		public void Add(KeyValuePair<string, T> item)
		{
			if (Pointers.ContainsKey(item.Key))
				throw new InvalidOperationException(nameof(item.Key));
			Constants.Add(item.Key, item.Value);
		}

		public void Add(string key, Func<T> value)
		{
			if (Constants.ContainsKey(key))
				throw new InvalidOperationException(nameof(key));
			Pointers.Add(key, value);
		}


		public void Clear()
		{
			Pointers.Clear();
			Constants.Clear();
		}

		public bool Contains(KeyValuePair<string, T> item)
		{
			if (Constants.TryGetValue(item.Key, out T value))
				if (value.Equals(item.Value))
					return true;
			return false;
		}

		public bool ContainsKey(string key)
		{
			return Constants.ContainsKey(key) || Pointers.ContainsKey(key);
		}

		void ICollection<KeyValuePair<string, T>>.CopyTo(KeyValuePair<string, T>[] array, int arrayIndex)
		{
			throw new NotImplementedException();
		}

		public IEnumerator<KeyValuePair<string, T>> GetEnumerator()
		{
			using (var enumerator = Constants.AsEnumerable().GetEnumerator())
				while (enumerator.MoveNext())
					yield return enumerator.Current;
			// XML parser doesn't like it
			//using (var enumerator = pointers.AsEnumerable().GetEnumerator())
			//	while (enumerator.MoveNext())
			//		yield return new KeyValuePair<string, T>(enumerator.Current.Key, enumerator.Current.Value.Invoke());
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			using (var enumerator = Constants.AsEnumerable().GetEnumerator())
				while (enumerator.MoveNext())
					yield return enumerator.Current;
			// XML parser doesn't like it
			//using (var enumerator = pointers.AsEnumerable().GetEnumerator())
			//	while (enumerator.MoveNext())
			//		yield return new KeyValuePair<string, T>(enumerator.Current.Key, enumerator.Current.Value.Invoke());
		}

		public bool Remove(string key)
		{
			if (Constants.ContainsKey(key))
				return Constants.Remove(key);
			if (Pointers.ContainsKey(key))
				return Pointers.Remove(key);
			return false;
		}

		bool ICollection<KeyValuePair<string, T>>.Remove(KeyValuePair<string, T> item)
		{
			throw new NotImplementedException();
		}

		public bool TryGetValue(string key, out T value)
		{
			if (Constants.TryGetValue(key, out value))
				return true;
			if (Pointers.TryGetValue(key, out Func<T> pointer))
			{
				value = pointer.Invoke();
				return true;
			}
			return false;
		}
	}

	/// <summary>
	/// This is what the script writers interact with the game, beside setting
	/// the actual script themselves.
	/// </summary>
	[Serializable]
	public sealed class ScriptDocumentInterface
	{
		

		/// <summary>
		/// This is used due to <see cref="XmlSerializer"/> is often used for this.
		/// Create a new interface from here.
		/// </summary>
		/// <returns></returns>
		public static ScriptDocumentInterface New()
		{
			var @interface = new ScriptDocumentInterface
			{
				strings = new Collection<string>(),
				PlayerName = "MC",
				bools = new Collection<bool>()
				{
					["True"] = true,
					["False"] = false,
					[additiveNameKey] = false,
				},
				ints = new Collection<int>(),
				floats = new Collection<float>()
			};
			@interface.bools.Add("henable", HentaiEnabled);
			bool HentaiEnabled() => B1NARYConfig.HEnable;
			return @interface;
		}

		public Collection<string> strings;
		public const string playerNameKey = "Player Name";
		public string PlayerName
		{
			get => strings[playerNameKey];
			set => strings[playerNameKey] = value;
		}
		public Collection<bool> bools;
		public const string additiveNameKey = "Additive";
		public bool Additive
		{
			get => bools[additiveNameKey];
			set => bools[additiveNameKey] = value;
		}
		public Collection<int> ints;
		public Collection<float> floats;

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
			bool HentaiEnabled() => B1NARYConfig.HEnable;
		}
	}
}