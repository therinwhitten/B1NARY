namespace B1NARY.Scripting.Experimental
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Threading.Tasks;
	using UnityEngine;
	using B1NARY.UI;
	using System.Linq;
	using B1NARY.Audio;
	using B1NARY.DesignPatterns;
	using UnityEngine.InputSystem;
	using System.Threading;

	public class ScriptHandler : Singleton<ScriptHandler>
	{
		public static readonly IEnumerable<KeyValuePair<string, Delegate>> Commands = new Dictionary<string, Delegate>()
		{
			["additive"] = (Action<string>)(boolRaw =>
			{
				bool setting;
				boolRaw = boolRaw.ToLower().Trim();
				if (ScriptDocument.enabledHashset.Contains(boolRaw))
					setting = true;
				else if (ScriptDocument.disabledHashset.Contains(boolRaw))
					setting = false;
				else
					throw new InvalidCastException(boolRaw);
				Instance.scriptDocument.AdditiveEnabled = setting;
			}),
			["changescript"] = (Action<string>)(scriptPath =>
			{
				Instance.InitializeNewScript(Application.streamingAssetsPath + '/' + scriptPath);
			}),
			["usegameobject"] = (Action<string>)(gameObjectName =>
			{
				GameObject @object = GameObject.Find(gameObjectName);
				if (@object == null)
					throw new MissingMemberException($"Gameobject '{gameObjectName}' is not found");
				@object.SetActive(true);
				Instance.ShouldPause = true;
				Task.Run(() =>
				{
					SpinWait.SpinUntil(() => !@object.activeSelf);
					Instance.ShouldPause = false;
				}).FreeBlockPath();
			}),
		};
		public static ScriptNode GetDefinedScriptNodes(ScriptDocument document, ScriptPair[] subLines)
		{
			if (subLines.First().LineType != ScriptLine.Type.Command)
				return null;
			string command = ((Command)subLines.First().scriptLine).command;
			switch (command.Trim().ToLower())
			{
				case "if":
					return new IfBlock(document, subLines);
				case "choice":
					return new ChoiceBlock(document, subLines);
			}
			return null;
		}
		/// <summary> Gets the name of the script loaded. </summary>
		public string ScriptName { get; private set; } = string.Empty;
		/// <summary> 
		/// A read-only version of the currently stored 
		/// <see cref="Experimental.ScriptDocument"/>. 
		/// </summary>
		public ScriptDocument ScriptDocument => scriptDocument;
		private ScriptDocument scriptDocument;
		[Tooltip("Where the script starts when it initializes a new script.")]
		public string StartupScriptPath;
		public PlayerInput playerInput;
		public string[] nextLineButtons;

		/// <summary>
		/// If the game should pause input. Modifying the variable 
		/// can be stacked with other scripts to allow compatibility. This comes 
		/// into play if you a script allows it to continue, but another says it
		/// shouldn't. In this case, it will read <see langword="true"/>.
		/// </summary>
		public bool ShouldPause 
		{ 
			get => m_pauseIterations > 0;
			set { if (value) m_pauseIterations++; else m_pauseIterations = checked(m_pauseIterations - 1); }
		}
		private uint m_pauseIterations = 0;
		/// <summary>
		/// A value that determines if it is running a script and ready to use.
		/// </summary>
		public bool IsActive { get; private set; } = false;
		/// <summary> Gets the current line. </summary>
		public ScriptLine CurrentLine => scriptDocument.CurrentLine;
		protected override void SingletonAwake()
		{
			foreach (string key in nextLineButtons)
				playerInput.actions.FindAction(key, true).performed += context => NextLine();
			DontDestroyOnLoad(gameObject);
		}

		public void InitializeNewScript(string scriptPath = "")
		{
			string finalPath = string.IsNullOrWhiteSpace(scriptPath) ? StartupScriptPath : scriptPath;
			var scriptFactory = new ScriptDocument.Factory(finalPath);
			scriptFactory.AddNodeParserFunctionality(GetDefinedScriptNodes);
			scriptFactory.AddCommandFunctionality(AudioController.Commands);
			scriptFactory.AddCommandFunctionality(SceneManager.Commands);
			scriptFactory.AddCommandFunctionality(ScriptHandler.Commands);
			scriptFactory.AddCommandFunctionality(B1NARY.CharacterController.Commands);
			scriptFactory.AddCommandFunctionality(TransitionManager.Commands);
			scriptDocument = scriptFactory.Parse(false);
			IsActive = true;
		}
		/// <summary>
		/// Plays lines until it hits a normal dialogue or similar.
		/// </summary>
		/// <returns> The <see cref="ScriptLine"/> it stopped at. </returns>
		public ScriptLine NextLine()
		{
			if (ShouldPause || scriptDocument == null)
				return default;
			try
			{
				return scriptDocument.NextLine();
			}
			catch (IndexOutOfRangeException)
			{
				IsActive = false;
				throw;
			}
		}
	}
}