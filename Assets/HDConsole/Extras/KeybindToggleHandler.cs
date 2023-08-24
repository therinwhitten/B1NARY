namespace HDConsole
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using UnityEngine;
	using UnityEngine.EventSystems;
	using UnityEngine.InputSystem;
	using UnityEngine.InputSystem.Controls;
	using UnityEngine.InputSystem.LowLevel;
	using static global::HDConsole.KeybindHandler;

	internal static class KeybindToggleHandler
	{
		public record BoundToggleValue(string EnableCommand, string DisableCommand, InputAction Action, bool Enabled)
		{
			public string TargetCommand => Enabled ? EnableCommand : DisableCommand;
			public string Serialize() => $"{Action.name}\n{EnableCommand}\n{DisableCommand}\n{Enabled}";
			public static BoundToggleValue Deserialize(string parsable)
			{
				string[] split = parsable.Split('\n');
				string key = split[2];
				KeyControl keyControl = Keyboard.current.FindKeyOnCurrentKeyboardLayout(key);
				InputAction action = new(key, InputActionType.Button);
				action.AddBinding(keyControl);
				action.performed += (context) =>
				{
					if (!HDConsole.Instance.enabled)
						InvokeKeybind(key);
				};
				return new(split[0], split[1], action, bool.Parse(split[3]));
			}
		}

		[return: CommandsFromGetter]
		public static HDCommand GetHDCommand() =>
			new("bind_toggle", new string[] { "key", "command", "enable", "disable" }, (args) =>
			{
				BindNewKey(args[0], args[1], args[2], args[3]);
			}) { description = "creates a new toggle. Is disabled by default, so pressing the button enables.", };

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
		private static void Constructor()
		{
			const string KEY = "HDConsole Toggle Keybinds";
			try
			{
				string output = PlayerPrefs.GetString(KEY, null);
				if (!string.IsNullOrEmpty(output))
					DeserializeFromString(output);

			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
				PlayerPrefs.DeleteKey(KEY);
			}

			Application.quitting += () =>
			{
				PlayerPrefs.SetString(KEY, SerializeAsString(boundToggles));
				for (int i = 0; i < boundCommands.Values.Count; i++)
					boundCommands.Values[i].Action.Dispose();
			};
		}

		public static SortedList<string, BoundToggleValue> boundToggles = new();



		internal static void InvokeKeybind(string key)
		{
			BoundToggleValue command = boundToggles[key];
			command = boundToggles[key] = new BoundToggleValue(command.EnableCommand, command.DisableCommand, command.Action, !command.Enabled);
			HDConsole.Instance.InvokeFromString($"{command} {command}");
		}
		public static void BindNewKey(string key, string command, string enableMod, string disableMod)
		{
			if (boundToggles.ContainsKey(key))
				RemoveKey(key);
			string enableCommand = $"{command} {enableMod}";
			string disableCommand = $"{command} {disableMod}";
			KeyControl keyControl = Keyboard.current.FindKeyOnCurrentKeyboardLayout(key);
			InputAction action = new(key, InputActionType.Button);
			action.AddBinding(keyControl);
			action.performed += (context) =>
			{
				if (!HDConsole.Instance.enabled)
					InvokeKeybind(key);
			};
			action.Enable();
			boundToggles[key] = new BoundToggleValue(enableCommand, disableCommand, action, false);
		}
		public static void RemoveKey(string key)
		{
			BoundToggleValue boundCommandValue = boundToggles[key];
			boundCommandValue.Action.Dispose();
			boundToggles.Remove(key);
		}
		public static void RemoveAll()
		{
			for (int i = 0; i < boundToggles.Values.Count; i++)
				boundToggles.Values[i].Action.Dispose();
			boundToggles.Clear();
		}


		private static void DeserializeFromString(string serializedString)
		{
			string[] split = serializedString.Split('\r');
			for (int i = 0; i < split.Length - 1; i++)
			{
				BoundToggleValue value = BoundToggleValue.Deserialize(split[i]);
				value.Action.Enable();
				boundToggles[value.Action.name] = value;
			}
		}
		private static string SerializeAsString(IEnumerable<KeyValuePair<string, BoundToggleValue>> values)
		{
			StringBuilder output = new();
			using var enumerable = values.GetEnumerator();
			while (enumerable.MoveNext())
			{
				string commandOutput = enumerable.Current.Value.Serialize();
				output.Append($"{commandOutput}\r");
			}
			return output.ToString();
		}
	}
}
