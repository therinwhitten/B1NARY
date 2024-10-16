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
	using System.Globalization;

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
		public static CommandArray Commands = new()
		{
			["textspeed"] = (Action<string>)(speedMultRaw =>
			{
				float multiplier = float.Parse(speedMultRaw);
				Instance.TickMultiplier = multiplier;
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
				Additive = setting;
			}),
		};

		public float TickMultiplier = 1f;
		private float m_secondsChar;
		public WaitForSeconds WaitSecondsPerChar => new(m_secondsChar);

		/// <summary>
		/// If the current dialogue should be added instead of skipping to a newString
		/// line. Uses <see cref="ScriptHandler.ScriptDocument"/> in order to store
		/// the field.
		/// </summary>
		public static bool Additive
		{
			get => SaveSlot.ActiveSlot.Additive;
			set
			{
				if (Additive == value)
					return;
				SaveSlot.ActiveSlot.Additive = value;
				if (value)
					Instance.CurrentText = string.Empty;
			}
		}
		public TMP_Text textBox;


		/// <summary>
		/// A property that directly points to the text box of <see cref="Text"/>.
		/// Use <see cref="CharacterManager.ActiveCharacterName"/> instead 
		/// for a more accurate name, as this can change visually within the game.
		/// </summary>
		//public string SpeakerName
		//{
		//	get => speakerBox.text;
		//	set
		//	{
		//		speakerBox.text = value.Replace(SaveSlot.DEFAULT_NAME, SaveSlot.ActiveSlot.PlayerName);
		//	}
		//}
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
		public static bool AutoSkip
		{
			get => m_autoSkip;
			set
			{
				if (m_autoSkip == value)
					return;
				FastSkip = false;
				m_autoSkip = value;
				if (!CoroutineWrapper.IsNotRunningOrNull(Instance.eventCoroutine))
					Instance.eventCoroutine.Stop();
				if (value)
					Instance.eventCoroutine = new CoroutineWrapper(Instance, AutoSkipCoroutine()).Start();

				static IEnumerator AutoSkipCoroutine()
				{
					while (AutoSkip)
					{
						yield return new WaitForEndOfFrame();
						if (!CoroutineWrapper.IsNotRunningOrNull(Instance.speakCoroutine))
							continue;
						if (CharacterManager.Instance.CharactersInScene.Any(character => character.controller.Mouths.Any(mouth => mouth.Value.IsPlaying)))
							continue;
						ScriptHandler.Instance.NextLine();
					}
				}
			}

		}
		private static bool m_autoSkip = false;
		/// <summary>
		/// Helper method that inverts <see cref="AutoSkip"/>, used by 
		/// <see cref="UnityEngine.Events.UnityEvent"/>
		/// </summary>
		public void SetAutoSkip(bool autoSkip) => AutoSkip = autoSkip;

		/// <summary>
		/// A toggle between if it should move foward within a fast, fixed 
		/// timeframe. Regardless of what is the current status of the text and 
		/// audio speech.
		/// </summary>
		public static bool FastSkip
		{
			get => m_fastSkip;
			set
			{
				if (m_fastSkip == value)
					return;
				AutoSkip = false;
				m_fastSkip = value;
				if (!CoroutineWrapper.IsNotRunningOrNull(Instance.eventCoroutine))
					Instance.eventCoroutine.Stop();
				if (value)
					Instance.eventCoroutine = new CoroutineWrapper(Instance, FastSkipCoroutine()).Start();

				static IEnumerator FastSkipCoroutine()
				{
					while (FastSkip)
					{
						yield return new WaitForSeconds(0.15f);
						ScriptHandler.Instance.NextLine();
					}
				}
			}
		}
		private static bool m_fastSkip = false;
		/// <summary>
		/// Helper method that inverts <see cref="FastSkip"/>, used by 
		/// <see cref="UnityEngine.Events.UnityEvent"/>
		/// </summary>
		public void SetFastSkip(bool value) => FastSkip = value;

		private string NewLine()
		{
			if (Additive)
				return CurrentText + (string.IsNullOrEmpty(CurrentText) || CurrentText.EndsWith(" ") ? "" : " ");
			return string.Empty;
		}

		private CoroutineWrapper eventCoroutine;
		private CoroutineWrapper speakCoroutine;
		public bool IsSpeaking => !CoroutineWrapper.IsNotRunningOrNull(speakCoroutine);

		protected override void SingletonAwake()
		{
			PlayerConfig.Instance.dialogueSpeedTicks.AttachValue((integer) =>
			{
				m_secondsChar = (integer * TickMultiplier) / 1000f;
			});
			if (FastSkip)
			{
				FastSkip = false;
				FastSkip = true;
				GameObject.Find("Fast Forward").GetComponent<Toggle>().isOn = true;
			}
			if (AutoSkip)
			{
				AutoSkip = false;
				AutoSkip = true;
				GameObject.Find("Autoskip").GetComponent<Toggle>().isOn = true;
			}
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
			FinalText = FinalText.Replace(SaveSlot.DEFAULT_NAME, SaveSlot.ActiveSlot.PlayerName);
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
					if (PlayerConfig.Instance.dialogueSpeedTicks.Value > 0)
						yield return WaitSecondsPerChar;
				}
			}
			CurrentText = FinalText;
		}

		public void QuickSave()
		{
			SaveSlot.Quicksave();
		}
		public void QuickLoad()
		{
			SaveSlot.ActiveSlot.Load();
		}
		public void NextLine()
		{
			ScriptHandler.Instance.NextLine();
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
			EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(DialogueSystem.textBox)));
			serializedObject.ApplyModifiedProperties();
		}

	}
}
#endif