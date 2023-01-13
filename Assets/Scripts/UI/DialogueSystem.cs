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
	using CharacterController = B1NARY.CharacterManagement.CharacterController;
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
		public static IEnumerable<KeyValuePair<string, Delegate>> Commands = new Dictionary<string, Delegate>()
		{
			["textspeed"] = (Action<string>)(speedRaw =>
			{
				Instance.TicksPerCharacter = int.Parse(speedRaw);
			}),
			/* There is already a system in ScriptHandler that handles more of it.
			["additive"] = (Action<string>)(str =>
			{
				if (ScriptDocument.enabledHashset.Contains(str))
					Instance.additiveTextEnabled = true;
				else if (ScriptDocument.disabledHashset.Contains(str))
					Instance.additiveTextEnabled = false;
				else 
					throw newString Exception();
			}),*/
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
		public bool AdditiveTextEnabled
		{
			get => ScriptHandler.Instance.ScriptDocument.AdditiveEnabled;
			set => ScriptHandler.Instance.ScriptDocument.AdditiveEnabled = value;
		}
		public TMP_Text speakerBox, textBox;


		/// <summary>
		/// A property that directly points to the text box of <see cref="Text"/>.
		/// Use <see cref="CharacterController.ActiveCharacterName"/> instead 
		/// for a more accurate name, as this can change visually within the game.
		/// </summary>
		public string SpeakerName
		{
			get => speakerBox.text;
			set => speakerBox.text = value;
		}
		private void ChangeSpeakerName(ICharacterController characterController)
		{
			if (characterController.CharacterName == "MC" && !string.IsNullOrEmpty(SaveSlot.Instance.data.PlayerName))
				SpeakerName = SaveSlot.Instance.data.PlayerName;
			else
				SpeakerName = characterController.CharacterName;
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
						if (CharacterController.Instance.charactersInScene.Values.Any(pair => pair.characterScript.VoiceData.IsPlaying))
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
			if (AdditiveTextEnabled)
				return CurrentText + ' ';
			return string.Empty;
		}

		private CoroutineWrapper eventCoroutine;
		private CoroutineWrapper speakCoroutine;

		private void Awake()
		{
			m_secondsChar = m_ticksPerChar / 1000f;
			CharacterController.Instance.ActiveCharacterChanged += ChangeSpeakerName;
		}

		/// <summary>
		/// Starts the speaking coroutine to say it.
		/// </summary>
		/// <param name="message"> The message to say. </param>
		public void Say(string message)
		{
			if (!DateTimeTracker.IsAprilFools)
				StopSpeaking(!AdditiveTextEnabled);
			speakCoroutine = new CoroutineWrapper(this, Speaking(message)).Start();
		}

		public void Say(string message, string speaker)
		{
			CharacterController.Instance.ChangeActiveCharacter(speaker);
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
		/// <see cref="AdditiveTextEnabled"/>. kept in mind, as it can add over
		/// previous text.
		/// </summary>
		/// <param name="speech"> The newString text to add to. </param>
		/// <returns> <see cref="WaitForSeconds"/> with <see cref="WaitSecondsPerChar"/>. </returns>
		private IEnumerator Speaking(string speech)
		{
			CurrentText = NewLine();
			FinalText = NewLine() + speech;
			if (FinalText.Contains("MC") && !string.IsNullOrEmpty(SaveSlot.Instance.data.PlayerName))
				FinalText = FinalText.Replace("MC", SaveSlot.Instance.data.PlayerName);
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
					yield return WaitSecondsPerChar;
				}
			}
			CurrentText = FinalText;
		}

		public void QuickSave()
		{
			SaveSlot.Instance.Serialize();
		}
		public void QuickLoad()
		{
			SaveSlot.QuickLoad();
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