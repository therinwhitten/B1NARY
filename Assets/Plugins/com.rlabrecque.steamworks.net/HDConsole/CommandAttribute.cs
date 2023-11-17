namespace HDConsole
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using System.Text;
	using System.Threading.Tasks;

	/// <summary>
	/// A method or property that changes how the game functions. 
	/// </summary>
	/// <remarks>
	/// Not although
	/// this is easier to implement, it is considered to be more resource-intensive
	/// instead of using <see cref="CommandsFromGetterAttribute"/>!
	/// </remarks>
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
	public class CommandAttribute : Attribute
	{
		private string CommandName { get; init; } = null;
		public string Description { get; init; }
		public CommandAttribute()
		{

		}
		public CommandAttribute(string name)
		{
			CommandName = name;
		}
		public CommandAttribute(string name, string description)
		{
			CommandName = name;
			Description = description;
		}

		public static bool TryParseMethod(MethodInfo info, out HDCommand command)
		{
			if (info.IsStatic == false)
			{
				command = default;
				return false;
			}
			Attribute[] attributes = GetCustomAttributes(info);
			int index = Array.FindIndex(attributes, att => att is CommandAttribute);
			if (index == -1)
			{
				command = default;
				return false;
			}
			CommandAttribute realAttribute = attributes[index] as CommandAttribute;
			ParameterInfo[] parameters = info.GetParameters();
			Type[] parseTo = new Type[parameters.Length];
			List<string> required = new(),
				optional = new();
			for (int i = 0; i < parameters.Length; i++)
			{
				parseTo[i] = parameters[i].ParameterType;
				if (typeof(IConvertible).IsAssignableFrom(parameters[i].ParameterType) == false)
				{
					command = default;
					return false;
				}
				if (parameters[i].HasDefaultValue)
					optional.Add(parameters[i].Name);
				else
					required.Add(parameters[i].Name);
			}
			bool hasReturns = info.ReturnType != typeof(void);
			string commandName = string.IsNullOrWhiteSpace(realAttribute.CommandName) ? info.Name : realAttribute.CommandName;

			command = new HDCommand(commandName, required, optional, (args) =>
			{
				object[] arguments = new object[parameters.Length];
				for (int i = 0; i < args.Length; i++)
				{
					if (parseTo[i] == typeof(string))
						arguments[i] = args[i];
					else
						arguments[i] = Convert.ChangeType(args[i], parseTo[i]);
				}
				for (int i = args.Length; i < arguments.Length; i++)
				{
					arguments[i] = parameters[i].DefaultValue;
				}
				object output = info.Invoke(null, arguments);
				if (hasReturns)
				{
					HDCommand.lastObjectGet = output;
					HDConsole.WriteLine($"{commandName} {output}");
				}
			}) { description = realAttribute.Description };
			return true;
		}
		public static bool TryParseProperty(PropertyInfo info, out HDCommand command)
		{
			if ((info.GetMethod ?? info.SetMethod).IsStatic == false)
			{
				command = default;
				return false;
			}
			Attribute[] attributes = GetCustomAttributes(info);
			int index = Array.FindIndex(attributes, att => att is CommandAttribute);
			if (index == -1)
			{
				command = default;
				return false;
			}
			CommandAttribute realAttribute = attributes[index] as CommandAttribute;
			Type targetType = info.PropertyType;
			if (typeof(IConvertible).IsAssignableFrom(targetType) == false)
			{
				command = default;
				return false;
			}
			List<string> required = new(),
				optional = new();
			if (info.GetMethod is not null && info.SetMethod is not null)
				optional.Add("value");
			else if (info.GetMethod is null)
				required.Add("value");
			else { } // Do nothing
			string commandName = string.IsNullOrWhiteSpace(realAttribute.CommandName) ? info.Name : realAttribute.CommandName;

			command = new HDCommand(commandName, required, optional, (args) =>
			{
				// Get
				if (args.Length == 0)
				{
					object value = info.GetMethod.Invoke(null, Array.Empty<object>());
					HDCommand.lastObjectGet = value;
					if (HDConsole.Instance.enabled)
						HDConsole.WriteLine($"{commandName} {value}");
					return;
				}
				// Set
				object[] values = new object[1];
				if (targetType == typeof(string))
					values[0] = args[0];
				else
					values[0] = Convert.ChangeType(args[0], targetType);
				info.SetMethod.Invoke(null, values);

			}) { description = realAttribute.Description };
			return true;
		}
	}
}
