namespace B1NARY.Scripting
{
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using System;
	using System.Text;
	using System.Data;

	public sealed class OverloadableCommand : IGrouping<string, Delegate>, IEquatable<OverloadableCommand>
	{
		// Operators
		/// <summary>
		/// Adds several delegates to the command, uses the name of 
		/// <paramref name="left"/>.
		/// </summary>
		/// <param name="left"> Source command. </param>
		/// <param name="right"> The delegates to add. </param>
		/// <returns> The overloaded comamnd with the delegates inside. </returns>
		public static OverloadableCommand operator +(OverloadableCommand left, OverloadableCommand right)
		{
			return left + right.AsEnumerable();
		}
		/// <summary>
		/// Adds several delegates to the command.
		/// </summary>
		/// <param name="left"> Source command. </param>
		/// <param name="right"> The delegates to add. </param>
		/// <returns> The overloaded comamnd with the delegates inside. </returns>
		public static OverloadableCommand operator +(OverloadableCommand left, IEnumerable<Delegate> right)
		{
			using (var enumerator = right.GetEnumerator())
				while (enumerator.MoveNext())
					left.AddOverload(enumerator.Current);
			return left;
		}
		/// <summary>
		/// Adds a delegate to the command.
		/// </summary>
		/// <param name="left"> Source command. </param>
		/// <param name="right"> The delegate to add. </param>
		/// <returns> The overloaded comamnd with the delegate inside. </returns>
		public static OverloadableCommand operator +(OverloadableCommand left, Delegate right)
		{
			left.AddOverload(right);
			return left;
		}
		/// <summary>
		/// Checks for exact equality: has null checking built-in. Checks the name,
		/// delegate count, then the delegates themselves.
		/// </summary>
		public static bool operator ==(OverloadableCommand left, OverloadableCommand right)
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
		/// <summary>
		/// Checks for opposite exact equality: has null checking built-in. Checks the name,
		/// delegate count, then the delegates themselves.
		/// </summary>
		public static bool operator !=(OverloadableCommand left, OverloadableCommand right)
		{
			return !(left == right);
		}
		public static implicit operator OverloadableCommand(Delegate @delegate)
		{
			return new OverloadableCommand(@delegate);
		}

		public static bool Invoke(IReadOnlyDictionary<string, OverloadableCommand> commands, (string command, string[] arguments) command)
		{
			return Invoke(commands, command.command, command.arguments);
		}

		public static bool Invoke(IReadOnlyDictionary<string, OverloadableCommand> commands, string commandKey, params string[] arguments)
		{
			if (commands == null || arguments == null)
				throw new ArgumentNullException();
			if (!commands.TryGetValue(commandKey, out OverloadableCommand activeCommand))
				throw new MissingMemberException($"Inputted commands does not contain a command in the keys with '{commandKey}'!");
			return Invoke(activeCommand, arguments);
		}
		/// <summary>
		/// Invokes a command externally; mean't for use for things like the
		/// <see cref="ScriptDocument"/>.
		/// </summary>
		/// <param name="command"> The command to invoke. </param>
		/// <param name="arguments"> The arguments. </param>
		/// <returns> If it should forcefully pause. </returns>
		/// <exception cref="ArgumentNullException"/>
		public static bool Invoke(OverloadableCommand command, params string[] arguments)
		{
			if (command == null || arguments == null)
				throw new ArgumentNullException();
			Delegate @delegate = command.Invoke(arguments);
			return @delegate.Method.GetCustomAttributes<ForcePauseAttribute>().Any();
		}

		/// <summary>
		/// All invokable commmands, should only have string as its parameters.
		/// </summary>
		private List<Delegate> delegates;
		/// <summary>
		/// Takes the length of a delegate as key, and spits out an index
		/// for use in <see cref="delegates"/>.
		/// </summary>
		private Dictionary<int, int> lengthShortcut;
		/// <summary>
		/// The Command Name.
		/// </summary>
		public string Name { get; private set; }
		/// <summary>
		/// The Command Name.
		/// </summary>
		string IGrouping<string, Delegate>.Key => Name;

		/// <summary>
		/// The count of all the overloaded delegates.
		/// </summary>
		public int DelegateCount => delegates.Count;

		public OverloadableCommand(string methodName)
		{
			delegates = new List<Delegate>(1);
			lengthShortcut = new Dictionary<int, int>();
			Name = methodName;
		}
		public OverloadableCommand(Delegate command) : this(command.Method.Name, command)
		{

		}
		public OverloadableCommand(string methodname, Delegate command)
		{
			delegates = new List<Delegate>(1);
			lengthShortcut = new Dictionary<int, int>();
			AddOverload(command);
			Name = methodname;
		}
		public OverloadableCommand(string methodName, OverloadableCommand source)
		{
			delegates = source.delegates;
			lengthShortcut = source.lengthShortcut;
			Name = methodName;
		}
		public OverloadableCommand(OverloadableCommand source)
		{
			delegates = source.delegates;
			lengthShortcut = source.lengthShortcut;
			Name = source.Name;
		}



		public void AddOverload(Delegate command)
		{
			ParameterInfo[] info = command.Method.GetParameters();
			if (info.Any(param => param.ParameterType != typeof(string)))
				throw new InvalidCastException();
			int length = info.Length;
			if (lengthShortcut.ContainsKey(length))
				throw new DuplicateCommandException();
			delegates.Add(command);
			lengthShortcut.Add(length, delegates.Count - 1);
		}
		public bool Contains(Delegate @delegate)
		{
			return delegates.Contains(@delegate);
		}
		public bool TryGetDelegate(int argumentCount, out Delegate @delegate)
		{
			if (!lengthShortcut.TryGetValue(argumentCount, out int index))
			{
				@delegate = null;
				return false;
			}
			@delegate = delegates[index];
			return true;
		}

		public Delegate Invoke(params string[] arguments)
		{
			if (TryGetDelegate(arguments.Length, out Delegate @delegate))
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

		public IEnumerator<Delegate> GetEnumerator() => delegates.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => delegates.GetEnumerator();

		bool IEquatable<OverloadableCommand>.Equals(OverloadableCommand other) => this == other;
	}


	[Serializable]
	public class DuplicateCommandException : Exception
	{
		public DuplicateCommandException() : base("There is a command with the same parameters being combined as one.")
		{
			HResult = 52962061;
		}
		public DuplicateCommandException(string message) : base(message)
		{
			HResult = 52962061;
		}
		public DuplicateCommandException(string message, Exception inner) : base(message, inner)
		{
			HResult = 52962061;
		}
		protected DuplicateCommandException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{
			HResult = 52962061;
		}
	}
}