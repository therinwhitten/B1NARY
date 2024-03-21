namespace HDConsole
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using UnityEngine;
	using UnityEngine.Events;

	/// <summary>
	/// Sets or invokes existing commands.
	/// </summary>
	public class ScriptCommandInvoker : MonoBehaviour
	{
		public string commandLine = string.Empty;

		public void InvokeCommand()
		{
			HDConsole.InvokeThoughConsole(commandLine);
		}
		public void InvokeCommand(string commandLine)
		{
			HDConsole.InvokeThoughConsole(commandLine);
		}

		public void MergeCommandWithValue(string secondValue)
		{
			HDConsole.InvokeThoughConsole(commandLine, secondValue);
		}
		public void MergeCommandWithValue(bool secondValue)
		{
			HDConsole.InvokeThoughConsole(commandLine, secondValue ? "1" : "0");
		}
		public void MergeCommandWithValue(float secondValue)
		{
			HDConsole.InvokeThoughConsole(commandLine, secondValue.ToString());
		}
		public void MergeCommandWithValue(int secondValue)
		{
			HDConsole.InvokeThoughConsole(commandLine, secondValue.ToString());
		}
		public void MergeCommandWithValue(Enum secondValue)
		{
			HDConsole.InvokeThoughConsole(commandLine, secondValue.GetHashCode().ToString());
		}
	}
}
