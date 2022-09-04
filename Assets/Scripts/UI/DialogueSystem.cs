namespace B1NARY.UI
{
	using System.Threading.Tasks;
	using System.Threading;
	using UnityEngine;
	using UnityEngine.UI;
	using DesignPatterns;
	using System.Collections;
	using System.Linq;
	using System.Collections.Generic;
	using System.Text;
	using System;
	using B1NARY.Scripting.Experimental;

	[RequireComponent(typeof(FadeController))]
	public class DialogueSystem : SingletonAlt<DialogueSystem>
	{
		public static IReadOnlyDictionary<string, Delegate> DialogueDelegateCommands = new Dictionary<string, Delegate>()
		{
			/* There is already a system in ScriptHandler that handles more of it.
			["additive"] = (Action<string>)(str =>
			{
				if (ScriptDocument.enabledHashset.Contains(str))
					Instance.additiveTextEnabled = true;
				else if (ScriptDocument.disabledHashset.Contains(str))
					Instance.additiveTextEnabled = false;
				else 
					throw new Exception();
			}),
			*/
		};

		

		/// <summary>
		/// A property that directly points to the text box of <see cref="Text"/>
		/// </summary>
		public string CurrentSpeaker
		{
			get => speakerBox.text;
			set => speakerBox.text = value;
		}
		public Text speakerBox, textBox;
		/// <summary>
		/// A property that directly points to the text box of <see cref="Text"/>
		/// </summary>
		public string CurrentText
		{
			get => textBox.text;
			private set => textBox.text = value;
		}
		private FadeController fadeController;

		[HideInInspector]
		public bool additiveTextEnabled = false;

		[HideInInspector] public bool isWaitingForUserInput = false;
		/// <summary> A task that gradually reveals the text. </summary>
		/// <returns> The final speech <see cref="string"/>. Not affected by abruptly stopping. </returns>
		public Task<string> SpeakingTask = Task.FromResult(string.Empty);
		private CancellationTokenSource tokenSource = new CancellationTokenSource();

		private void Awake()
		{
			fadeController = GetComponent<FadeController>();
		}
		private void Start()
		{
			CurrentSpeaker = string.Empty;
			CurrentText = string.Empty;
		}
		public void FadeIn(float fadeTime = 0.5f)
		{
			fadeController.FadeIn(fadeTime);
		}
		public void FadeOut(float fadeTime = 0.5f)
		{
			fadeController.FadeOut(fadeTime);
		}

		/// <summary>
		/// Say something and show it on the speech box. rich text works or otherwise.
		/// </summary>
		public void Say(string speech)
		{
			StopSpeaking(true).Wait();
			SpeakingTask = Speaking(speech, tokenSource.Token);
		}
		/// <summary>
		/// Say something and show it on the speech box. rich text works or otherwise.
		/// </summary>
		public void Say(string speech, string speaker)
		{
			CurrentSpeaker = speaker;
			StopSpeaking(true).Wait();
			SpeakingTask = Speaking(speech, tokenSource.Token);
		}

		/// <summary>
		/// Stops the speaker <see cref="Task"/>. 
		/// </summary>
		/// <param name="setFinalResultToCurrentText">
		/// <see langword="true"/> to set the intended speech to reach, <see langword="false"/>
		/// if it will be set as empty. <see langword="null"/> if it just resides as
		/// the same.
		/// </param>
		public async Task StopSpeaking(bool? setFinalResultToCurrentText)
		{
			tokenSource.Cancel();
			await SpeakingTask;
			if (setFinalResultToCurrentText != null)
				if (setFinalResultToCurrentText == true)
					CurrentText = SpeakingTask.Result;
				else
					CurrentText = string.Empty;
			tokenSource = new CancellationTokenSource();
		}

		private string NewLine()
		{
			if (additiveTextEnabled)
				return CurrentText + ' ';
			return string.Empty;
		}

		protected override void OnSingletonDestroy()
		{
			_ = StopSpeaking(null);
		}

		/// <summary>
		/// A <see cref="Task"/> that reveals <paramref name="speech"/> over time.
		/// </summary>
		/// <param name="speech">The speech to reveal.</param>
		/// <param name="token">The token to stop it.</param>
		/// <param name="lengthPerChar">Milliseconds to wait per character.</param>
		/// <returns> <paramref name="speech"/>. </returns>
		private async Task<string> Speaking(string speech, CancellationToken token, int lengthPerChar = 30)
		{
			CurrentText = NewLine();
			int tagsLength = 0;
			for (int i = 0; i < speech.Length && !token.IsCancellationRequested; i++)
			{
				if (speech[i] == '<') // Tag starts
				{
					// Building the tag
					var tag = new StringBuilder();
					// TODO: Add a check that will catch an out of bounds exception.
					while (speech[i] != '>')
					{
						tag.Append(speech[i]);
						i++;
					}
					tag.Append('>');
					string tagFull = tag.ToString();
					// Behaviour among the tag.
					if (tagFull[1] != '/')
					{
						string endTag = tagFull.Insert(1, "/");
						CurrentText += tagFull + endTag;
						tagsLength += endTag.Length;
					}
					else
						tagsLength -= tagFull.Length;
					continue;
				}
				CurrentText = CurrentText.Insert(CurrentText.Length - tagsLength, speech[i].ToString());
				await Task.Delay(lengthPerChar);
			}
			isWaitingForUserInput = true;
			return speech;
		}

	}
}