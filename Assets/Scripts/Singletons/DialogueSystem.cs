namespace B1NARY.UI
{
	using System.Threading.Tasks;
	using System.Threading;
	using UnityEngine;
	using UnityEngine.UI;
	using DesignPatterns;
	using System.Collections;
	using System;
	using System.Collections.Generic;
	using System.Text;

	[RequireComponent(typeof(FadeController))]
	public class DialogueSystem : SingletonAlt<DialogueSystem>
	{

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
			set => textBox.text = value;
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
			B1NARYConsole.Log(name, "Initializing");
			fadeController.FadeIn(fadeTime);
		}
		public void FadeOut(float fadeTime = 0.5f)
		{
			B1NARYConsole.Log(name, "De-Initializing");
			fadeController.FadeOut(fadeTime);
		}

		/// <summary>
		/// Say something and show it on the speech box. rich text works or otherwise.
		/// </summary>
		public void Say(string speech)
		{
			StopSpeaking(false).Wait();
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
			_ = StopSpeaking(false);
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
			const int outline = 2;
			CurrentText = NewLine();
			ushort accumulativeTagsLength = 0;
			await this.StartAwaitableCoroutine(SpeakOverTime());
			IEnumerator SpeakOverTime()
			{
				StringBuilder newTag = null;
				for (int i = 0; i < speech.Length && !token.IsCancellationRequested; i++)
				{
					char character = speech[i];
					if (newTag != null) // if it is in a tag
					{
						if (character == '>')
						{
							if (newTag.ToString()[0] == '/')
								accumulativeTagsLength = checked((ushort)(accumulativeTagsLength - 
									newTag.Length + outline - 1));
							else
								accumulativeTagsLength += (ushort)(newTag.Length + outline);
							newTag = null;
						}
						else
							newTag.Append(character);
						continue;
					}
					if (character == '<')
					{
						newTag = new StringBuilder();
						continue;
					}
					CurrentText = CurrentText.Insert(CurrentText.Length - accumulativeTagsLength, character.ToString());
					yield return new WaitForSecondsRealtime(lengthPerChar / 1000f);
				}
			}
			isWaitingForUserInput = true;
			return speech;
		}

	}
}