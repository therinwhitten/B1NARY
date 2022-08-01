namespace B1NARY
{
	using System;
	using UI;
	using Logging;
	using System.Collections.Generic;
	using System.Linq;
	using UnityEngine;
	using UnityEngine.InputSystem;
	using UnityEngine.UI;

	[RequireComponent(typeof(FadeController), typeof(CanvasGroup))]
	public class B1NARYConsole : MonoBehaviour, ITogglableInterface
	{

		public string ActionName => "Open B1NARYConsole";
		private static void WriteLine(string condition, string trackTrace, LogType logType)
		{
			if (printedInUnityConsole)
			{
				printedInUnityConsole = false;
				return;
			}
			WriteLine(new Prefix(logType), condition + '\n' + trackTrace);
		}

		public static void Log(params string[] arguments)
		{
			if (arguments.Length > 1)
				WriteLine(new Prefix(arguments.Take(arguments.Length - 1), LogType.Log), arguments.Last(), false);
			else
				WriteLine(new Prefix(LogType.Log), arguments.Single(), false);
		}
		public static void LogWarning(params string[] arguments)
		{
			if (arguments.Length > 1)
				WriteLine(new Prefix(arguments.Take(arguments.Length - 1), LogType.Warning), arguments.Last(), true);
			else
				WriteLine(new Prefix(LogType.Warning), arguments.Single(), true);
		}
		public static void LogError(params string[] arguments)
		{
			if (arguments.Length > 1)
				WriteLine(new Prefix(arguments.Take(arguments.Length - 1), LogType.Error), arguments.Last(), true);
			else
				WriteLine(new Prefix(LogType.Error), arguments.Single(), true);
		}

		public static void WriteLine(IEnumerable<string> prefixes, string message,
			LogType log = LogType.Log, bool printInUnityConsole = false) =>
			WriteLine(new Prefix(prefixes, log), message, printInUnityConsole);
		private static bool printedInUnityConsole = false;
		public static void WriteLine(Prefix prefix, string message, bool printInUnityConsole = false)
		{
			string output = prefix.ToString() + ' ' + message;
			if (printInUnityConsole && !printedInUnityConsole)
			{
				printedInUnityConsole = true;
				switch (prefix.logType)
				{
					case LogType.Log:
						Debug.Log(output);
						break;
					case LogType.Warning:
						Debug.LogWarning(output);
						break;
					case LogType.Exception:
					case LogType.Error:
						Debug.LogError(output);
						break;
				}
			}
			else if (printedInUnityConsole)
			{
				printedInUnityConsole = false;
				return;
			}
			AddedNewLine?.Invoke(output);
		}
		private static event Action<string> AddedNewLine;



		[SerializeField]
		private PlayerInput playerInput;
		private FadeController fadeController;
		private CanvasGroup canvasGroup;
		[SerializeField]
		private Text text;
		private Queue<string> cache;

		private void Awake()
		{
			fadeController = GetComponent<FadeController>();
			canvasGroup = GetComponent<CanvasGroup>();
			ITogglableInterface @interface = this;
			playerInput.actions.FindAction(@interface.ActionName, true).performed
					+= @interface.TogglePlayerOpen;
			AddedNewLine += AddItem;
			Application.logMessageReceived += WriteLine;
		}
		private void AddItem(string inputLine)
		{
			if (canvasGroup.blocksRaycasts)
				UpdateText(inputLine);
			else
			{
				if (cache == null)
					cache = new Queue<string>();
				cache.Enqueue(inputLine);
			}
		}
		private void UpdateText(string newLine)
		{
			text.text += '\n' + newLine;
		}

		void ITogglableInterface.TogglePlayerOpen(InputAction.CallbackContext context)
		{
			if (canvasGroup.blocksRaycasts)
				_ = fadeController.FadeOut(0.2f);
			else
			{
				_ = fadeController.FadeIn(0.2f);
				if (cache != null)
				{
					while (cache.Count > 0)
						UpdateText(cache.Dequeue());
					cache = null;
				}
			}
		}

		
	}
}
namespace B1NARY.Logging
{
	using System.Linq;
	using System.Collections.Generic;
	using UnityEngine;

	public struct Prefix
	{
		public LogType logType;
		private List<string> prefixes;
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