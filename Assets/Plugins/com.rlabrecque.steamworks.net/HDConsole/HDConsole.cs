﻿namespace HDConsole
{
	using OVSSerializer.IO;
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Text;
	using TMPro;
	using UnityEditor;
	using UnityEngine;
	using UnityEngine.Diagnostics;
	using UnityEngine.InputSystem;
	using UnityEngine.SocialPlatforms.Impl;
	using UnityEngine.UI;

	public class HDConsole : Singleton<HDConsole>
	{
		internal record ActiveButton(HDOtherCommand @Object, string ChangesTo);

		public static void WriteLine(LogType type, object input)
		{
			if (!HasInstance)
				return;
			Color color = Instance.GetColor(type);
			WriteLine(color, input);
		}
		public static void WriteLine(object input) => WriteLine(LogType.Log, input);
		public static void WriteLine(Color color, object input)
		{
			if (!HasInstance)
				return;
			Instance.AddText($"{ToStringConsole(input?.ToString(), color)}\n");
		}

		public static void Write(object input) => Write(LogType.Log, input);
		public static void Write(Color color, object input)
		{
			if (!HasInstance)
				return;
			Instance.AddText(ToStringConsole(input?.ToString(), color));
		}
		public static void Write(LogType type, object input)
		{
			if (!HasInstance)
				return;
			Color color = Instance.GetColor(type);
			Write(color, input);
		}
		public static void InvokeThoughConsole(params string[] arguments)
		{
			if (!HasInstance)
			{
				Debug.LogWarning("Cannot invoke commands through the console if the console doesn't exist!");
				return;
			}
			Instance.InvokeFromString(string.Join(" ", arguments));
		}

		public static event Func<bool> InvokingModeratorCommand
		{
			add => allModActions.Add(value);
			remove => allModActions.Remove(value);
		}
		private static readonly List<Func<bool>> allModActions = new();
		private static bool CanInvokeModCommand() => allModActions.Any(action => action.Invoke() == false) == false;
		public static Func<bool> CheatsEnabled { get; set; } = () => false;

		private static string ToStringConsole(string input, Color color) =>
			$"<color=#{ColorUtility.ToHtmlStringRGBA(color)}>{input}</color>";

		#region Constructors
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		private static void Preconstructor() => Application.logMessageReceived += PreQueue;
		private static void PreQueue(string condition, string stackTrace, LogType type) => preQueueLogs.Enqueue((condition, stackTrace, type));
		private static Queue<(string condition, string stackTrace, LogType type)> preQueueLogs = new();

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void Constructor()
		{
			if (HasInstance)
				return;
			GameObject @object = Instantiate(Resources.Load<GameObject>("HDConsole"));
			if (@object.TryGetComponent(out HDConsole console))
				Instance = console;
			Application.logMessageReceived -= PreQueue;
			Application.logMessageReceived += WriteToConsole;
			while (preQueueLogs.Count > 0)
			{
				var (condition, stackTrace, type) = preQueueLogs.Dequeue();
				WriteToConsole(condition, stackTrace, type);
			}
			preQueueLogs = null;

			static void WriteToConsole(string condition, string stackTrace, LogType type)
			{
				WriteLine(type, $"<b>{condition}</b>");
				if (type != LogType.Log)
					Write(type, stackTrace);
			}
		}
		#endregion


		/// <summary> Action that controls if the console opens or closes. </summary>
		public InputAction openClose;
		/// <summary> Going up in <see cref="queuedCommands"/> queue. </summary>
		public InputAction up;
		/// <summary> Going down in <see cref="queuedCommands"/> queue. </summary>
		public InputAction down;
		/// <summary> Setting input field to current <see cref="queuedCommands"/> queue. </summary>
		public InputAction tab;

		/// <summary> The general UI to disable or enable. </summary>
		public GameObject consoleUI;

		[Space]
		public TMP_Text consoleText;
		public Scrollbar slider;
		public TMP_Text description;
		public static int LineCapacity
		{
			get => PlayerPrefs.GetInt("HDConsole Line Capacity", byte.MaxValue);
			set => PlayerPrefs.SetInt("HDConsole Line Capacity", value);
		}

		[Space]
		public RectTransform otherCommandListLocation;
		/// <summary>
		/// How many commands the console can display at any given time. 
		/// Note that this doesn't influence of how many actual commands are given.
		/// </summary>
		[Tooltip("How many commands the console can display at any given time. Note that this doesn't influence of how many actual commands are given.")]
		public int displayOtherCommandAmount = 5;
		public HDOtherCommand template;

		[Space]
		public Color LogColor = Color.white;
		public Color LogErrorColor = Color.white;
		public Color WarningColor = Color.yellow;
		public Color ErrorColor = Color.red;
		public Color AssertColor = Color.magenta;
		public Color GetColor(LogType type) => type switch
		{
			LogType.Log => LogColor,
			LogType.Warning => WarningColor,
			LogType.Assert => AssertColor,
			LogType.Error => ErrorColor,
			LogType.Exception => ErrorColor,
			_ => throw new IndexOutOfRangeException(type.ToString())
		};

		internal readonly LinkedList<string> consoleTextMemory = new();
		internal event Action<string> AddedLine;
		internal LinkedList<string> consoleCommandMemory;
		internal const string MEM_CMD = "HDConsole Command Memory";
		internal const char MEM_SPLIT_KEY = '\r';

		internal static OSFile HDLogPath => new(Path.Combine(Application.persistentDataPath, "HDConsole.log"));
		internal LogAutoSyncer syncer;

		internal List<HDCommand> commands;
		internal Dictionary<string, HDCommand> commandDict;

		internal List<ActiveButton> activeButtons;
		internal List<string> queuedCommands = new();
		internal int QueuedCommandsIndex
		{
			get
			{
				while (m_queuedCommandsIndex < 0)
					m_queuedCommandsIndex += queuedCommands.Count;
				return m_queuedCommandsIndex %= queuedCommands.Count;
			}

			set => m_queuedCommandsIndex = value;
		}
		private int m_queuedCommandsIndex = 0;
		public string CommandFromIndex => queuedCommands[QueuedCommandsIndex];



		public TMP_InputField inputField;
		public IReadOnlyList<string> CommandLine
		{
			get => inputField.text.Split(';');
			set => inputField.text = string.Join(';', value);
		}

		private void Reset()
		{
			consoleText = GetComponentInChildren<TMP_Text>();
			inputField = GetComponentInChildren<TMP_InputField>();
		}

		protected override void SingletonAwake()
		{
			DontDestroyOnLoad(gameObject);
			consoleCommandMemory = new LinkedList<string>();
			string memory = PlayerPrefs.GetString(MEM_CMD, null);
			if (memory != null)
			{
				string[] splitCommands = memory.Split(MEM_SPLIT_KEY);
				for (int i = 0; i < splitCommands.Length; i++)
					consoleCommandMemory.AddLast(splitCommands[i]);
			}
			commands = CommandsFromGetterAttribute.GetList();
			activeButtons = new List<ActiveButton>(displayOtherCommandAmount);
			commands = new List<HDCommand>(commands.OrderBy(command => command.command));
			commandDict = commands.ToDictionary(command => command.command);
			openClose.started += OnOpenClose;
			openClose.Enable();
			
			//syncer = new(HDLogPath, this, TimeSpan.FromSeconds(10f));
		}

		private void OnEnable()
		{
			consoleUI.SetActive(true);
			inputField.onValueChanged.AddListener(ChangedInputField);
			inputField.onSubmit.AddListener(OnSubmit);
			up.started += MoveUp;
			down.started += MoveDown;
			tab.started += OnTab;
			up.Enable();
			down.Enable();
			tab.Enable();
			UpdateOtherCommandList();
			UpdateDescriptivePanel();
			//StartCoroutine(RenableDelay(description.transform.parent.gameObject, true));
		}
		private void OnDisable()
		{
			consoleUI.SetActive(false);
			inputField.onValueChanged.RemoveListener(ChangedInputField);
			inputField.onSubmit.RemoveListener(OnSubmit);
			up.started -= MoveUp;
			down.started -= MoveDown;
			tab.started -= OnTab;
			up.Disable();
			down.Disable();
			tab.Disable();
		}
		protected override void OnSingletonDestroy()
		{
			openClose.started -= OnOpenClose;
			PlayerPrefs.SetString(MEM_CMD, string.Join(MEM_SPLIT_KEY, consoleCommandMemory));
			PlayerPrefs.Save();
			//syncer.Dispose();
		}

		private void AddText(string input)
		{
			//if (slider != null && slider.value < 0.05f)
			//	StartCoroutine(SetToEnd());
			consoleTextMemory.AddLast(input);
			while (consoleTextMemory.Count > LineCapacity)
				consoleTextMemory.RemoveFirst();
			StringBuilder builder = new();
			for (LinkedListNode<string> node = consoleTextMemory.First; node != null; node = node.Next)
				builder.Append(node.Value);
			consoleText.text = builder.ToString();
			AddedLine?.Invoke(input);

			//IEnumerator SetToEnd()
			//{
			//	yield return new WaitForEndOfFrame();
			//	slider.value = 0f;
			//}
		}

		// Player Input
		private void OnOpenClose(InputAction.CallbackContext callbackContext)
		{
			enabled = !enabled;
			if (enabled)
				inputField.ActivateInputField();
		}
		private void ChangedInputField(string newValue)
		{
			UpdateOtherCommandList();
		}
		private void MoveUp(InputAction.CallbackContext callbackContext)
		{
			m_allowUpdateList = false;
			if (inputField.text.StartsWith(CommandFromIndex.Split(' ')[0]))
				QueuedCommandsIndex -= 1; 
			FinishCommandFromClick(CommandFromIndex);
			m_allowUpdateList = true;
			UpdateDescriptivePanel();
		}
		private void MoveDown(InputAction.CallbackContext callbackContext)
		{
			m_allowUpdateList = false;
			if (inputField.text.StartsWith(CommandFromIndex.Split(' ')[0]))
				QueuedCommandsIndex += 1;
			FinishCommandFromClick(CommandFromIndex);
			m_allowUpdateList = true;
			UpdateDescriptivePanel();
		}
		private void OnTab(InputAction.CallbackContext callbackContext)
		{
			if (queuedCommands.Count == 0)
				return;
			FinishCommandFromClick(CommandFromIndex);
		}

		public void OnSubmit() => OnSubmit(inputField.text);
		public void OnSubmit(string input)
		{
			inputField.text = string.Empty;
			inputField.ActivateInputField();
			if (string.IsNullOrEmpty(input))
				return;
			WriteLine(input);
			consoleCommandMemory.Remove(input);
			consoleCommandMemory.AddFirst(input);
			InvokeFromString(input);
			UpdateOtherCommandList();
			QueuedCommandsIndex = 0;
		}


		// Updating values
		private bool m_allowUpdateList = true;
		/// <summary>
		/// Updates both <see cref="queuedCommands"/> and <see cref="activeButtons"/>
		/// in order to suggest certain commands for the player to use.
		/// </summary>
		/// <remarks>
		/// Also updates the command description.
		/// </remarks>
		internal void UpdateOtherCommandList()
		{
			if (m_allowUpdateList == false)
				return;
			queuedCommands.Clear();
			bool isCommands = !string.IsNullOrWhiteSpace(CommandLine[^1]);
			if (isCommands)
				// Existing commands that match closest
				for (int i = 0; i < commands.Count; i++)
				{
					if (!commands[i].command.StartsWith(CommandLine[^1].TrimStart().Split(' ')[0]))
						continue;
					queuedCommands.Add(commands[i].ToString());
				}
			else
				// Just using the most recent commands instead.
				queuedCommands.AddRange(consoleCommandMemory);

			// Check if they need a replacement, via length first
			int newLength = Math.Min(queuedCommands.Count, displayOtherCommandAmount);
			bool revampButtons = false;
			if (activeButtons.Count != newLength)
				revampButtons = true;
			else
				for (int i = 0; i < activeButtons.Count; i++)
				{
					if (queuedCommands[i] == activeButtons[i].ChangesTo)
						continue;
					revampButtons = true;
					break;
				}

			// Remove and re-add buttons
			if (!revampButtons)
				return;
			activeButtons.ForEach(button => button.Object.Destroy());
			activeButtons.Clear();
			for (int i = 0; i < newLength; i++)
			{
				ActiveButton newButton = new(template.DuplicateTo(otherCommandListLocation), queuedCommands[i]);
				newButton.Object.text.text = queuedCommands[i];
				newButton.Object.button.onClick.AddListener(() => FinishCommandFromClick(newButton.ChangesTo));
				activeButtons.Add(newButton);
			}

			//StartCoroutine(RenableDelay(otherCommandListLocation.gameObject));
			UpdateDescriptivePanel();
		}
		/// <summary>
		/// Updates the panel given based on the index of queuedCommands.
		/// </summary>
		internal void UpdateDescriptivePanel()
		{
			if (queuedCommands.Count <= 0 || string.IsNullOrWhiteSpace(CommandLine[^1]))
			{
				description.text = "";
				description.gameObject.SetActive(false);
				return;
			}

			string changesTo = CommandFromIndex;
			changesTo = changesTo.Split(' ')[0];
			if (!commandDict.TryGetValue(changesTo, out HDCommand closestRelative))
			{
				description.text = "";
				description.gameObject.SetActive(false);
				return;
			}

			StringBuilder builder = new();
			// Description
			if (!string.IsNullOrWhiteSpace(closestRelative.description))
				builder.Append($"{closestRelative.description}\n");
			builder.Append(closestRelative.ToString());
			string output = builder.ToString();
			//bool changed = description.text == output;
			description.text = output;
			//if (changed)
			description.gameObject.SetActive(true);
			//else
			//	StartCoroutine(RenableDelay(description.gameObject));
		}


		public void FinishCommandFromClick(string changesTo)
		{
			// If it is just a formal command; not from the player
			if (!consoleCommandMemory.Contains(changesTo))
				changesTo = changesTo.Split(' ')[0];

			inputField.ActivateInputField();
			List<string> set = new(CommandLine);
			set[^1] = set.Count > 1 ? $" {changesTo}" : changesTo;
			CommandLine = set;
			inputField.stringPosition = changesTo.Length;
		}




		public void InvokeFromString(string fullCommand)
		{
			if (fullCommand.Length <= 0)
				return;
			string[] allCommands = fullCommand.Split(';');
			string[][] splitCommands = allCommands.Select(str => HDCommand.SplitWithQuotations(str)).ToArray();
			StartCoroutine(CommandStream());

			IEnumerator CommandStream()
			{
				for (int i = 0; i < allCommands.Length; i++)
				{
					string[] splitCommand = splitCommands[i];
					if (!commandDict.TryGetValue(splitCommand[0], out HDCommand command))
					{
						WriteLine(LogErrorColor, $"Command '{splitCommand[0]}' is not found");
						yield break;
					}
					if (command.mainTags.HasFlag(HDCommand.MainTags.ServerModOnly) && !CanInvokeModCommand())
					{
						WriteLine(LogErrorColor, $"Command '{splitCommand[0]}' cannot be invoked by a non-moderator.");
						yield break;
					}
					if (command.mainTags.HasFlag(HDCommand.MainTags.Cheat) && !CheatsEnabled.Invoke())
					{
						WriteLine(LogErrorColor, $"Command '{splitCommand[0]}' is a cheat, but cheats are not enabled.");
						yield break;
					}
					int argumentCount = splitCommand.Length - 1;
					if (command.requiredArguments.Count > argumentCount)
					{
						WriteLine(LogErrorColor, $"Command '{splitCommand[0]}' has missing required arguments not fullfilled");
						yield break;
					}

					if (KeybindHandler.Delay.command == splitCommand[0])
					{
						if (float.TryParse(splitCommand[1], out float seconds))
							yield return new WaitForSecondsRealtime(seconds);
						continue;
					}

					try
					{
						command.Invoke(splitCommand.Skip(1).ToArray());
					}
					catch (Exception ex)
					{
						WriteLine(LogType.Exception, ex.ToString());
						yield break;
					}
				}
			}
		}
	}
}