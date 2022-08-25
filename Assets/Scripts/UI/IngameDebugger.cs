namespace B1NARY
{
	using System;
	using B1NARY.UI;
	using System.Collections.Generic;
	using System.Linq;
	using UnityEngine;
	using UnityEngine.InputSystem;
	using UnityEngine.UI;
	using B1NARY.DesignPatterns;

	[RequireComponent(typeof(FadeController), typeof(CanvasGroup))]
	public class IngameDebugger : Multiton<IngameDebugger>, ITogglableInterface
	{
		[ExecuteAlways]
		private static void StaticConstructor()
		{
			Application.logMessageReceived += Log;

		} 

		public static void Log(params string[] arguments) =>
			Log(new Prefix(arguments.Take(arguments.Length - 1), LogType.Log), arguments.Last());
		public static void LogWarning(params string[] arguments) =>
			Log(new Prefix(arguments.Take(arguments.Length - 1), LogType.Warning), arguments.Last());
		public static void LogError(params string[] arguments) =>
			Log(new Prefix(arguments.Take(arguments.Length - 1), LogType.Error), arguments.Last());
		private static void Log(string condition, string stackTrace, LogType type)
		{
			var prefix = new Prefix(type);
			string message = prefix.ToString() + condition + '\n' + stackTrace;
			Log(prefix, message);

		}
		private static bool printedUnityObject = false;
		public static void Log(Prefix prefix, string message)
		{
			if (printedUnityObject)
			{
				printedUnityObject = false;
				return;
			}
			printedUnityObject = true;
			switch (prefix.logType)
			{
				case LogType.Log:
					Debug.Log(prefix.ToString() + message);
					break;
				case LogType.Error:
					Debug.LogError(prefix.ToString() + message);
					break;
				case LogType.Warning:
					Debug.LogWarning(prefix.ToString() + message);
					break;
			}
			IEnumerator<IngameDebugger> enumerator = GetEnumerator();
			while (enumerator.MoveNext())
				enumerator.Current.AddLine(message);
		}

		[SerializeField] private PlayerInput playerInput;
		[SerializeField] private Text textBox;
		private CanvasGroup canvasGroup;
		private FadeController fadeController;
		private void Awake()
		{
			canvasGroup = GetComponent<CanvasGroup>();
			fadeController = GetComponent<FadeController>();
			ITogglableInterface togglableInterface = this;
			playerInput.actions.FindAction(togglableInterface.ActionName, true).performed 
				+= togglableInterface.TogglePlayerOpen;
		}

		private void AddLine(string message)
		{
			textBox.text += '\n' + message;
		}

		string ITogglableInterface.ActionName => "Open IngameDebugger";
		void ITogglableInterface.TogglePlayerOpen(InputAction.CallbackContext context)
		{
			if (canvasGroup.blocksRaycasts)
				fadeController.FadeOut(0.2f);
			else
				fadeController.FadeIn(0.2f);
		}
	}
	public struct Prefix
	{
		public LogType logType;
		private List<string> prefixes;
		public Prefix(params string[] prefixes) : this(LogType.Log, prefixes)
		{ }
		public Prefix(LogType logType = LogType.Log, params string[] prefixes) : this(prefixes, logType)
		{ }
		public Prefix(IEnumerable<string> prefixes, LogType logType = LogType.Log)
		{
			this.prefixes = new List<string>(prefixes);
			this.logType = logType;
		}
		public void Add(string input)
			=> prefixes.Add(input);
		public override string ToString() => '[' +
			(logType != LogType.Log ? prefixes.Any() ? $"{logType}/" : logType.ToString() : "")
			+ string.Join("/", prefixes) + ']';
	}
}