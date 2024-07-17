namespace HDConsole
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using TMPro;
	using UnityEngine;

	/// <summary>
	/// Converts <see cref="HDCommand"/> into the console from returned lists, 
	/// single items, arrays, or other enumerable commands.
	/// </summary>
	[AttributeUsage(AttributeTargets.ReturnValue | AttributeTargets.Field, AllowMultiple = false)]
	public class CommandsFromGetterAttribute : Attribute
	{
		private static List<HDCommand> commands = null;
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
		private static void Constructor()
		{
			GetList();
		}
		public static List<HDCommand> GetList()
		{
			if (commands != null)
				return commands;
			List<HDCommand> Commands = new();
			HashSet<string> commandNames = new();
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			for (int i = 0; i < assemblies.Length; i++)
				Commands.AddRange(GetFromAssembly(assemblies[i]));
			for (int i = 0; i < Commands.Count; i++)
			{
				HDCommand currentCommand = Commands[i];
				if (commandNames.Contains(currentCommand.command))
				{
					Debug.LogWarning($"{currentCommand.command} is already implemented!");
					Commands.RemoveAt(i);
					i--;
					continue;
				}
				commandNames.Add(currentCommand.command);
			}
			return commands = Commands;
		}
		private static IList<HDCommand> GetFromAssembly(Assembly assembly)
		{
			List<HDCommand> output = new();
			Type[] classes = assembly.GetTypes();
			for (int i = 0; i < classes.Length; i++)
			{
				MemberInfo[] allMembers = classes[i].GetMembers(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
				for (int ii = 0; ii < allMembers.Length; ii++)
				{
					IList<HDCommand> commands = null;
					try
					{
						if (allMembers[ii] is MethodInfo meth)
						{
							if (meth.ReturnTypeCustomAttributes.IsDefined(typeof(CommandsFromGetterAttribute), true))
								commands = DefineValue(meth.Invoke(null, Array.Empty<object>()));
							else if (meth.IsDefined(typeof(CommandAttribute), true) && CommandAttribute.TryParseMethod(meth, out HDCommand command))
								commands = new HDCommand[] { command };
						}
						else if (allMembers[ii] is PropertyInfo poop)
						{
							if (poop.GetMethod.ReturnTypeCustomAttributes.IsDefined(typeof(CommandsFromGetterAttribute), true))
								commands = DefineValue(poop.GetValue(null, Array.Empty<object>()));
							else if (poop.IsDefined(typeof(CommandAttribute), true) && CommandAttribute.TryParseProperty(poop, out HDCommand command))
								commands = new HDCommand[] { command };
						}
						else if (allMembers[ii] is FieldInfo food && food.IsDefined(typeof(CommandsFromGetterAttribute), true))
							commands = DefineValue(food.GetValue(null));
					} catch { }

					if (commands is null)
						continue;
					// adding range
					for (int iii = 0; iii < commands.Count; iii++)
						output.Add(commands[iii]);
				}
			}
			return output;
		}

		private static IList<HDCommand> DefineValue(object input)
		{
			if (input is IList<HDCommand> commands)
				return commands;
			if (input is IEnumerable<HDCommand> enumerable)
				return enumerable.ToArray();
			return new HDCommand[] { (HDCommand)input };
		}
	}
}