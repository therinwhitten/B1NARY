namespace B1NARY.Scripting
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;


	public class CommandArray : IDictionary<string, OverloadableCommand>,
		IReadOnlyDictionary<string, OverloadableCommand>,
		IList<OverloadableCommand>, IReadOnlyList<OverloadableCommand>,
		IEnumerable<OverloadableCommand>, IEnumerable<Delegate>
	{
		public static explicit operator List<OverloadableCommand>(CommandArray arr)
		{
			return arr.commands;
		}

		ICollection<string> IDictionary<string, OverloadableCommand>.Keys => nameToDel.Keys;
		IEnumerable<string> IReadOnlyDictionary<string, OverloadableCommand>.Keys => nameToDel.Keys;
		ICollection<OverloadableCommand> IDictionary<string, OverloadableCommand>.Values => commands;
		IEnumerable<OverloadableCommand> IReadOnlyDictionary<string, OverloadableCommand>.Values => commands;

		public bool IsReadOnly => false;

		private readonly List<OverloadableCommand> commands;
		public IReadOnlyList<OverloadableCommand> Commands => commands;
		private readonly Dictionary<string, int> nameToDel;
		public ICollection<string> MethodNames => nameToDel.Keys;

		public int Count => commands.Count;

		public CommandArray()
		{
			commands = new List<OverloadableCommand>();
			nameToDel = new Dictionary<string, int>();
		}
		public CommandArray(int capacity)
		{
			commands = new List<OverloadableCommand>(capacity);
			nameToDel = new Dictionary<string, int>();
		}

		OverloadableCommand IList<OverloadableCommand>.this[int index]
		{
			get => commands[index];
			set => commands[index] = value;
		}
		OverloadableCommand IReadOnlyList<OverloadableCommand>.this[int index]
		{
			get => commands[index];
		}

		public OverloadableCommand this[string key]
		{
			get
			{
				if (!nameToDel.ContainsKey(key))
					throw new KeyNotFoundException($"'{key}' is not present in the command array!");
				return commands[nameToDel[key]];
			}

			set
			{
				if (nameToDel.ContainsKey(key))
					commands[nameToDel[key]] += value;
				else
					Add(key, value);
			}
		}

		public bool TryGetValue(string key, out OverloadableCommand value)
		{
			bool output = nameToDel.TryGetValue(key, out int index);
			value = index == -1 ? null : commands[index];
			return output; 
		}

		public bool ContainsKey(string key) => nameToDel.ContainsKey(key);
		public bool Contains(OverloadableCommand command)
		{
			return commands.Contains(command);
		}
		public bool Contains(Delegate @delegate)
		{
			for (int i = 0; i < commands.Count; i++)
				if (commands[i].Contains(@delegate))
					return true;
			return false;
		}
		public bool Contains(KeyValuePair<string, OverloadableCommand> item)
		{
			return Contains(new OverloadableCommand(item.Key, item.Value));
		}

		public int IndexOf(OverloadableCommand item)
		{
			return commands.IndexOf(item);
		}

		public void Add(OverloadableCommand command)
		{
			if (nameToDel.TryGetValue(command.Name, out int index))
				commands[index] += command;
			commands.Add(command);
			nameToDel.Add(command.Name, Count - 1);
		}
		public void Add(string methodName, Delegate firstDelegate, params Delegate[] delegates)
		{
			Add(methodName, firstDelegate, delegates.AsEnumerable());
		}
		public void Add(string methodName, Delegate firstDelegate, IEnumerable<Delegate> delegates)
		{
			var command = new OverloadableCommand(methodName, firstDelegate);
			using (var enumerator = delegates.GetEnumerator())
				while (enumerator.MoveNext())
					command.AddOverload(enumerator.Current);
			Add(command); 
		}
		public void Add(string key, OverloadableCommand value)
		{
			Add(key, value.First(), value.Skip(1));
		}
		public void Add(KeyValuePair<string, OverloadableCommand> item)
		{
			Add(item.Key, item.Value);
		}
		public void AddRange(params OverloadableCommand[] commands)
		{
			AddRange(commands.AsEnumerable());
		}
		public void AddRange(IEnumerable<OverloadableCommand> commands)
		{
			using (var enumerator = commands.GetEnumerator())
				while (enumerator.MoveNext())
					Add(enumerator.Current);
		}

		public void Clear()
		{
			commands.Clear();
			nameToDel.Clear();
		}
		public bool Remove(string key)
		{
			if (!ContainsKey(key))
				return false;
			RemoveAt(nameToDel[key]);
			return true;
		}
		public bool Remove(KeyValuePair<string, OverloadableCommand> keyValuePair)
		{
			return Remove(keyValuePair.Key);
		}
		public bool Remove(OverloadableCommand item)
		{
			int index = IndexOf(item);
			if (index == -1)
				return false;
			RemoveAt(index);
			return true;
		}
		public void RemoveAt(int index)
		{
			string name = commands[index].Name;
			commands.RemoveAt(index);
			nameToDel.Remove(name);
		}

		public IEnumerator<OverloadableCommand> GetEnumerator()
		{
			return commands.GetEnumerator();
		}

		IEnumerator<Delegate> IEnumerable<Delegate>.GetEnumerator()
		{
			for (int i = 0; i < commands.Count; i++)
				using (var enumerator = commands[i].GetEnumerator())
					while (enumerator.MoveNext())
						yield return enumerator.Current;
		}

		IEnumerator<KeyValuePair<string, OverloadableCommand>> IEnumerable<KeyValuePair<string, OverloadableCommand>>.GetEnumerator()
		{
			for (int i = 0; i < Count; i++)
				yield return new KeyValuePair<string, OverloadableCommand>(commands[i].Name, commands[i]);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}








		
		void ICollection<KeyValuePair<string, OverloadableCommand>>.CopyTo(KeyValuePair<string, OverloadableCommand>[] array, int arrayIndex)
		{
			throw new NotImplementedException();
		}
		void IList<OverloadableCommand>.Insert(int index, OverloadableCommand item)
		{
			throw new NotSupportedException();
		}

		void ICollection<OverloadableCommand>.CopyTo(OverloadableCommand[] array, int arrayIndex)
		{
			throw new NotSupportedException();
		}

	}
}