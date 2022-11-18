namespace B1NARY.Scripting
{
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using System;
	using System.Text;

	public sealed class OverloadableCommand<TDel> : IGrouping<string, TDel>, IEquatable<OverloadableCommand<TDel>> where TDel : Delegate
	{
		// Operators
		public static OverloadableCommand<TDel> operator +(OverloadableCommand<TDel> left, OverloadableCommand<TDel> right)
		{
			if (left.Name != right.Name)
				throw new InvalidOperationException();
			return left + right.AsEnumerable();
		}
		public static OverloadableCommand<TDel> operator +(OverloadableCommand<TDel> left, IEnumerable<TDel> right)
		{
			using (var enumerator = right.GetEnumerator())
				while (enumerator.MoveNext())
					left.AddOverload(enumerator.Current);
			return left;
		}
		public static OverloadableCommand<TDel> operator +(OverloadableCommand<TDel> left, TDel right)
		{
			left.AddOverload(right);
			return left;
		}
		public static bool operator ==(OverloadableCommand<TDel> left, OverloadableCommand<TDel> right)
		{
			if (left is null || right is null)
			{
				if (left is null && right is null)
					return true;
				return false;
			}
			if (left.Name != right.Name)
				return false;
			if (left.DelegateCount != right.DelegateCount)
				return false;
			for (int i = 0; i < left.delegates.Count; i++)
				if (left.delegates[i] != right.delegates[i])
					return false;
			return true;
		}
		public static bool operator !=(OverloadableCommand<TDel> left, OverloadableCommand<TDel> right)
		{
			return !(left == right);
		}
		public static implicit operator OverloadableCommand<TDel>(TDel @delegate)
		{
			return new OverloadableCommand<TDel>(@delegate);
		}

		public static bool Invoke(IReadOnlyDictionary<string, OverloadableCommand<TDel>> commands, (string command, string[] arguments) command)
			=> Invoke(commands, command.command, command.arguments);
		public static bool Invoke(IReadOnlyDictionary<string, OverloadableCommand<TDel>> commands, string commandKey, params string[] arguments)
		{
			if (!commands.TryGetValue(commandKey, out OverloadableCommand<TDel> activeCommand))
				throw new MissingMemberException($"Inputted commands does not contain a command in the keys with '{commandKey}'!");
			return Invoke(activeCommand, arguments);
		}
		public static bool Invoke(OverloadableCommand<TDel> command, params string[] arguments)
		{
			TDel @delegate = command.Invoke(arguments);
			return @delegate.Method.GetCustomAttributes<ForcePauseAttribute>().Any();
		}

		/// <summary>
		/// All invokable commmands, should only have string as its parameters.
		/// </summary>
		private List<TDel> delegates;
		/// <summary>
		/// Takes the length of a delegate as key, and spits out an index
		/// for use in <see cref="delegates"/>.
		/// </summary>
		private Dictionary<int, int> lengthShortcut;
		/// <summary>
		/// The Command Name.
		/// </summary>
		public string Name { get; private set; }
		string IGrouping<string, TDel>.Key => Name;

		public int DelegateCount => delegates.Count;

		public OverloadableCommand(string methodName)
		{
			delegates = new List<TDel>(1);
			Name = methodName;
		}
		public OverloadableCommand(TDel command) : this(command.Method.Name, command)
		{

		}
		public OverloadableCommand(string methodname, TDel command)
		{
			delegates = new List<TDel>(1);
			AddOverload(command);
			Name = methodname;
		}



		public void AddOverload(TDel command)
		{
			ParameterInfo[] info = command.Method.GetParameters();
			if (info.Any(param => param.ParameterType != typeof(string)))
				throw new InvalidCastException();
			int length = info.Length;
			if (lengthShortcut.ContainsKey(length))
				throw new Exception();
			delegates.Add(command);
			lengthShortcut.Add(length, delegates.Count - 1);
		}
		public bool Contains(TDel @delegate)
		{
			return delegates.Contains(@delegate);
		}
		public bool TryGetDelegate(int argumentCount, out TDel @delegate)
		{
			if (!lengthShortcut.TryGetValue(argumentCount, out int index))
			{
				@delegate = null;
				return false;
			}
			@delegate = delegates[index];
			return true;
		}

		public TDel Invoke(params string[] arguments)
		{
			if (TryGetDelegate(arguments.Length, out TDel @delegate))
			{
				@delegate.DynamicInvoke(arguments);
				return @delegate;
			}
			throw new InvalidOperationException("argument length of " +
				$"{arguments.Length} is not part of the following argument" +
				$" lengths: {string.Join(", ", delegates.Select(del => del.Method.GetParameters().Length))}");
		}

		public override bool Equals(object obj)
		{
			return base.Equals(obj);
		}
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
		/// <summary>
		/// 
		/// </summary>
		/// <returns>
		/// <c></c>
		/// </returns>
		public override string ToString() => new StringBuilder($"Command '{Name}': ")
			.Append(string.Join(",  ", delegates.Select(del => 
			new StringBuilder(del.Method.Name).Append("; L.")
			.Append(del.Method.GetParameters().Length).ToString()))).ToString();

		public IEnumerator<TDel> GetEnumerator() => delegates.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => delegates.GetEnumerator();

		bool IEquatable<OverloadableCommand<TDel>>.Equals(OverloadableCommand<TDel> other) => this == other;
	}
}