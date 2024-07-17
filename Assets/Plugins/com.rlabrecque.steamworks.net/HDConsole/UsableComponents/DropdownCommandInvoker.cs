namespace HDConsole
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using TMPro;
	using UnityEngine;
	using UnityEngine.Events;

	/// <summary>
	/// Specifically gets values
	/// </summary>
	public class CommandValueGetter : MonoBehaviour
	{
		public string command = string.Empty;
		public UnityEvent<int> intGetters;
		public UnityEvent<float> floatGetters;

		private void Start()
		{
			if (intGetters.GetPersistentEventCount() > 0)
			{
				HDConsole.InvokeThoughConsole(command);
				int output = (int)HDCommand.lastObjectGet;
				intGetters.Invoke(output);
			}
			if (floatGetters.GetPersistentEventCount() > 0)
			{
				HDConsole.InvokeThoughConsole(command);
				float output = (float)HDCommand.lastObjectGet;
				floatGetters.Invoke(output);
			}
		}
	}
}
#if UNITY_EDITOR
namespace HDConsole.Editor
{

}
#endif