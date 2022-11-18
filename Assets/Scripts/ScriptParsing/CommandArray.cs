namespace B1NARY.Scripting
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;


	public class CommandArray : IDictionary<string, OverloadableCommand<Delegate>>,
		IReadOnlyDictionary<string, OverloadableCommand<Delegate>>,
		IList<OverloadableCommand<Delegate>>, IReadOnlyList<OverloadableCommand<Delegate>>,
		IEnumerable<OverloadableCommand<Delegate>>, IEnumerable<Delegate>
	{
		public static explicit operator List<OverloadableCommand<Delegate>>(CommandArray arr)
		{
			return new List<OverloadableCommand<Delegate>>(arr.commands);
		}

		ICollection<string> IDictionary<string, OverloadableCommand<Delegate>>.Keys => nameToDel.Keys;
		IEnumerable<string> IReadOnlyDictionary<string, OverloadableCommand<Delegate>>.Keys => nameToDel.Keys;
		ICollection<OverloadableCommand<Delegate>> IDictionary<string, OverloadableCommand<Delegate>>.Values => commands;
		IEnumerable<OverloadableCommand<Delegate>> IReadOnlyDictionary<string, OverloadableCommand<Delegate>>.Values => commands;

		public bool IsReadOnly => false;

		private readonly List<OverloadableCommand<Delegate>> commands;
		public IReadOnlyList<OverloadableCommand<Delegate>> Commands => commands;
		private readonly Dictionary<string, int> nameToDel;
		public ICollection<string> MethodNames => nameToDel.Keys;

		public int Count => commands.Count;

		public CommandArray()
		{
			commands = new List<OverloadableCommand<Delegate>>();
			nameToDel = new Dictionary<string, int>();
		}
		public CommandArray(int capacity)
		{
			commands = new List<OverloadableCommand<Delegate>>(capacity);
			nameToDel = new Dictionary<string, int>();
		}

		OverloadableCommand<Delegate> IList<OverloadableCommand<Delegate>>.this[int index]
		{
			get => commands[index];
			set => commands[index] = value;
		}
		OverloadableCommand<Delegate> IReadOnlyList<OverloadableCommand<Delegate>>.this[int index]
		{
			get => commands[index];
		}

		public OverloadableCommand<Delegate> this[string key] 
		{ 
			get => commands[nameToDel[key]];
			set => commands[nameToDel[key]] = value;
		}

		public bool TryGetValue(string key, out OverloadableCommand<Delegate> value)
		{
			bool output = nameToDel.TryGetValue(key, out int index);
			value = index == -1 ? null : commands[index];
			return output; 
		}

		public bool ContainsKey(string key) => nameToDel.ContainsKey(key);
		public bool Contains(OverloadableCommand<Delegate> command)
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

		public int IndexOf(OverloadableCommand<Delegate> item)
		{
			return commands.IndexOf(item);
		}

		public void Add(OverloadableCommand<Delegate> command)
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
			var command = new OverloadableCommand<Delegate>(methodName, firstDelegate);
			using (var enumerator = delegates.GetEnumerator())
				while (enumerator.MoveNext())
					command.AddOverload(enumerator.Current);
			Add(command); 
		}
		public void Add(string key, OverloadableCommand<Delegate> value)
		{
			Add(key, value.First(), value.Skip(1));
		}
		public void Add(KeyValuePair<string, OverloadableCommand<Delegate>> item)
		{
			Add(item.Key, item.Value);
		}
		public void AddRange(params OverloadableCommand<Delegate>[] commands)
		{
			AddRange(commands.AsEnumerable());
		}
		public void AddRange(IEnumerable<OverloadableCommand<Delegate>> commands)
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
		public bool Remove(KeyValuePair<string, OverloadableCommand<Delegate>> keyValuePair)
		{
			return Remove(keyValuePair.Key);
		}
		public bool Remove(OverloadableCommand<Delegate> item)
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

		public IEnumerator<OverloadableCommand<Delegate>> GetEnumerator()
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






		

		

		bool ICollection<KeyValuePair<string, OverloadableCommand<Delegate>>>.Contains(KeyValuePair<string, OverloadableCommand<Delegate>> item)
		{
			throw new NotImplementedException();
		}

		void ICollection<KeyValuePair<string, OverloadableCommand<Delegate>>>.CopyTo(KeyValuePair<string, OverloadableCommand<Delegate>>[] array, int arrayIndex)
		{
			throw new NotImplementedException();
		}

		IEnumerator<KeyValuePair<string, OverloadableCommand<Delegate>>> IEnumerable<KeyValuePair<string, OverloadableCommand<Delegate>>>.GetEnumerator()
		{
			throw new NotImplementedException();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			throw new NotImplementedException();
		}


		

		void IList<OverloadableCommand<Delegate>>.Insert(int index, OverloadableCommand<Delegate> item)
		{
			throw new NotSupportedException();
		}

		void ICollection<OverloadableCommand<Delegate>>.CopyTo(OverloadableCommand<Delegate>[] array, int arrayIndex)
		{
			throw new NotSupportedException();
		}

	}
}