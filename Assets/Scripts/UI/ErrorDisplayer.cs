namespace B1NARY
{
	using UnityEngine;
	using UnityEngine.UI;

	[RequireComponent(typeof(Image))]
	public sealed class ErrorDisplayer : MonoBehaviour
	{
		private void Awake()
		{
			Application.logMessageReceived += LogMessageRecieved;
		}
		private void LogMessageRecieved(string condition, string stackTrace, LogType type)
		{

		}
	}
}