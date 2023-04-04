 namespace B1NARY.UI
{
	using UnityEngine;
	using UnityEngine.UI;
	using DesignPatterns;
	using System.Collections;
	using System.Linq;
	using System.Collections.Generic;
	using System.Text;
	using System;
	using TMPro;
	using CharacterManager = B1NARY.CharacterManagement.CharacterManager;
	using B1NARY.Scripting;
	using B1NARY.DataPersistence;
	using B1NARY.CharacterManagement;

	/// <summary>
	/// 
	/// </summary>
	public class DialogueSystem : Singleton<DialogueSystem>
	{
		/// <summary>
		/// A command that adds all the inputs together into a split list that
		/// separates from regular text and tags. <paramref name="original"/> 
		/// is considered a tag, regardless of it's status.
		/// </summary>
		/// <param name="original">
		/// The original text to stack upon, can be completely empty for a newString
		/// line.
		/// </param>
		/// <param name="newString"> The newString text to add. Tags can be used. </param>
		/// <returns> 
		/// A list of split <see cref="string"/>s, that tags is marked to intend
		/// that it will be shown instantly.
		/// </returns>
		public static List<(string value, bool isTag)> SplitDialogue(string original, string newString)
		{
			var parsableText = string.IsNullOrEmpty(original)
				? new List<(string value, bool isTag)>()
				: new List<(string value, bool isTag)>() { (original, true) };
			var activeSlot = new StringBuilder();
			for (int i = 0; i < newString.Length; i++)
			{
				if (newString[i] == '<')
				{
					parsableText.Add((activeSlot.ToString(), false));
					activeSlot = new StringBuilder();
				}
				activeSlot.Append(newString[i]);
				if (newString[i] == '>')
				{
					parsableText.Add((activeSlot.ToString(), true));
					activeSlot = new StringBuilder();
				}
			}
			if (activeSlot.Length > 0)
				parsableText.Add((activeSlot.ToString(), false));
			return parsableText;
		}
		public static CommandArray Commands = new CommandArray
		{
			["textspeed"] = (Action<string>)(speedRaw =>
			{
				Instance.TicksPerCharacter = int.Parse(speedRaw);
			}),
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
				Instance.Additive = setting;
			}),
		};

		public int TicksPerCharacter
		{
			get => m_ticksPerChar;
			set
			{
				m_ticksPerChar = Math.Max(0, value);
				m_secondsChar = m_ticksPerChar / 1000f;
			}
		}
		[Tooltip("How many ticks or milliseconds should the game wait per character?")]
		private int m_ticksPerChar = 30;
		private float m_secondsChar = 30f / 1000f;
		public WaitForSeconds WaitSecondsPerChar => new WaitForSeconds(m_secondsChar);

		/// <summary>
		/// If the current dialogue should be added instead of skipping to a newString
		/// line. Uses <see cref="ScriptHandler.ScriptDocument"/> in order to store
		/// the field.
		/// </summary>
		public bool Additive
		{
			get => SaveSlot.ActiveSlot.Additive;
			set
			{
				if (Additive == value)
					return;
				SaveSlot.ActiveSlot.Additive = value;
				if (value)
					CurrentText = string.Empty;
			}
		}
		public TMP_Text speakerBox, textBox;


		/// <summary>
		/// A property that directly points to the text box of <see cref="Text"/>.
		/// Use <see cref="CharacterManager.ActiveCharacterName"/> instead 
		/// for a more accurate name, as this can change visually within the game.
		/// </summary>
		public string SpeakerName
		{
			get => speakerBox.text;
			set => speakerBox.text = value;
		}
		private void ChangeSpeakerName(Character? characterController)
		{
			if (!characterController.HasValue)
				return;
			if (characterController.Value.controller.CharacterName == "MC")
				SpeakerName = SaveSlot.ActiveSlot.PlayerName;
			else
				SpeakerName = characterController.Value.controller.CharacterName;
		}
		/// <summary>
		/// A property that directly points to the text box of <see cref="Text"/>
		/// </summary>
		public string CurrentText
		{
			get => textBox.text;
			private set => textBox.text = value;
		}
		/// <summary>
		/// A property that tells what the final result of <see cref="CurrentText"/>
		/// should be when it finishes without interupption.
		/// </summary>
		public string FinalText { get; private set; }
		/// <summary>
		/// If both speaker and line are both finished playing and it should
		/// automatically move to the next line.
		/// </summary>
		public bool AutoSkip
		{
			get => m_autoSkip;
			set
			{
				if (m_autoSkip == value)
					return;
				FastSkip = false;
				m_autoSkip = value;
				if (!CoroutineWrapper.IsNotRunningOrNull(eventCoroutine))
					eventCoroutine.Stop();
				if (value)
					eventCoroutine = new CoroutineWrapper(this, AutoSkipCoroutine()).Start();

				IEnumerator AutoSkipCoroutine()
				{
					while (AutoSkip)
					{
						yield return new WaitForEndOfFrame();
						if (!CoroutineWrapper.IsNotRunningOrNull(speakCoroutine))
							continue;
						if (CharacterManager.Instance.CharactersInScene.Values.Any(pair => pair.controller.VoiceData.IsPlaying))
							continue;
						ScriptHandler.Instance.NextLine();
					}
				}
			}

		}
		private bool m_autoSkip = false;
		/// <summary>
		/// Helper method that inverts <see cref="AutoSkip"/>, used by 
		/// <see cref="UnityEngine.Events.UnityEvent"/>
		/// </summary>
		public void ToggleAutoSkip() => AutoSkip = !AutoSkip;

		/// <summary>
		/// A toggle between if it should move foward within a fast, fixed 
		/// timeframe. Regardless of what is the current status of the text and 
		/// audio speech.
		/// </summary>
		public bool FastSkip
		{
			get => m_fastSkip;
			set
			{
				if (m_fastSkip == value)
					return;
				AutoSkip = false;
				m_fastSkip = value;
				if (!CoroutineWrapper.IsNotRunningOrNull(eventCoroutine))
					eventCoroutine.Stop();
				if (value)
					eventCoroutine = new CoroutineWrapper(this, FastSkipCoroutine()).Start();

				IEnumerator FastSkipCoroutine()
				{
					while (FastSkip)
					{
						yield return new WaitForSeconds(0.15f);
						ScriptHandler.Instance.NextLine();
					}
				}
			}
		}
		private bool m_fastSkip = false;
		/// <summary>
		/// Helper method that inverts <see cref="FastSkip"/>, used by 
		/// <see cref="UnityEngine.Events.UnityEvent"/>
		/// </summary>
		public void ToggleFastSkip() => FastSkip = !FastSkip;

		private string NewLine()
		{
			if (Additive)
				return CurrentText + (string.IsNullOrEmpty(CurrentText) || CurrentText.EndsWith(" ") ? "" : " ");
			return string.Empty;
		}

		private CoroutineWrapper eventCoroutine;
		private CoroutineWrapper speakCoroutine;
		public bool IsSpeaking => !CoroutineWrapper.IsNotRunningOrNull(speakCoroutine);

		private void Awake()
		{
			m_secondsChar = m_ticksPerChar / 1000f;
			CharacterManager.Instance.ActiveCharacterChanged += ChangeSpeakerName;
		}

		/// <summary>
		/// Starts the speaking coroutine to say it.
		/// </summary>
		/// <param name="message"> The message to say. </param>
		public void Say(string message)
		{
			if (!DateTimeTracker.IsAprilFools)
				StopSpeaking(null);
			speakCoroutine = new CoroutineWrapper(this, Speaking(message)).Start();
		}

		public void Say(string message, string speaker)
		{
			CharacterManager.Instance.ChangeActiveCharacterViaName(speaker);
			Say(message);
		}

		/// <summary>
		/// Stops the speaking coroutine.
		/// </summary>
		/// <param name="setFinalResultToCurrentText">
		/// What to do when it completes the stopped coroutine.
		/// <list type="bullet">
		/// <item>
		///	<see langword="null"/>: Don't do anything relating to <see cref="CurrentText"/>
		/// </item>
		/// <item>
		///		<see langword="false"/>: Set the <see cref="CurrentText"/> to be
		///		completely empty
		/// </item>
		/// <item>
		/// <see langword="true"/>: Set the text from <see cref="FinalText"/>
		/// to <see cref="CurrentText"/>
		/// </item>
		/// </list>
		/// </param>
		public void StopSpeaking(bool? setFinalResultToCurrentText)
		{
			if (!CoroutineWrapper.IsNotRunningOrNull(speakCoroutine))
				speakCoroutine.Stop();
			if (setFinalResultToCurrentText != null)
				if (setFinalResultToCurrentText == true)
					CurrentText = FinalText;
				else
					CurrentText = string.Empty;
		}

		/// <summary>
		/// A coroutine that slowly type-writes the current text, with 
		/// <see cref="Additive"/>. kept in mind, as it can add over
		/// previous text.
		/// </summary>
		/// <param name="speech"> The newString text to add to. </param>
		/// <returns> <see cref="WaitForSeconds"/> with <see cref="WaitSecondsPerChar"/>. </returns>
		private IEnumerator Speaking(string speech)
		{
			CurrentText = NewLine();
			FinalText = NewLine() + speech;
			FinalText = FinalText.Replace("MC", SaveSlot.ActiveSlot.PlayerName);
			string speakerName = CharacterManager.Instance.ActiveCharacter?.controller.CharacterName;
			List<(string value, bool isTag)> parsableText = SplitDialogue(CurrentText, speech);

			string[] splitText = new string[parsableText.Count];
			// Adding all tags n' stuff beforehand.
			for (int i = 0; i < parsableText.Count; i++)
				if (parsableText[i].isTag)
					splitText[i] = parsableText[i].value;
			// Iterating per char here, printing them in the process.
			for (int i = 0; i < parsableText.Count; i++)
			{
				if (parsableText[i].isTag)
					continue;
				for (int ii = 0; ii < parsableText[i].value.Length; ii++)
				{
					splitText[i] += parsableText[i].value[ii];
					CurrentText = string.Join("", splitText);
					if (!(speakerName is null))
						SpeakerName = speakerName;
					yield return WaitSecondsPerChar;
				}
			}
			CurrentText = FinalText;
		}

		public void QuickSave()
		{
			SaveSlot.ActiveSlot.Save();
		}
		public void QuickLoad()
		{
			SaveSlot.ActiveSlot.Load();
		}
	} 
}
#if UNITY_EDITOR
namespace B1NARY.Editor
{
	using B1NARY.UI;
	using System.Threading.Tasks;
	using UnityEditor;
	using UnityEngine;

	[CustomEditor(typeof(DialogueSystem))]
	public class DialogueSystemEditor : Editor
	{
		private DialogueSystem dialogueSystem;
		private void Awake()
		{
			dialogueSystem = (DialogueSystem)target;
		}
		public override void OnInspectorGUI()
		{
			//EditorGUILayout.LabelField("Current Font: " + dialogueSystem.CurrentFontAsset.name);
			EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(DialogueSystem.speakerBox)));
			EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(DialogueSystem.textBox)));
			dialogueSystem.TicksPerCharacter = DirtyAuto.Slider(target, new GUIContent("Ticks Waited Per Character", "How long the text box will wait per character within milliseconds, with 0 being instantaneous. 30 by default."), dialogueSystem.TicksPerCharacter, 0, 200);
			serializedObject.ApplyModifiedProperties();
		}

	}
}
#endif