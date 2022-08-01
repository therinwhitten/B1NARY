namespace B1NARY.UI
{
	using System.Threading.Tasks;
	using System.Threading;
	using UnityEngine;
	using UnityEngine.UI;
	using DesignPatterns;

	[RequireComponent(typeof(FadeController))]
	public class DialogueSystem : SingletonAlt<DialogueSystem>
	{
		public static void Initialize() => FindObjectOfType<DialogueSystem>().enabled = true;
		public static void DeInitialize() => FindObjectOfType<DialogueSystem>().enabled = false;

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


		private void OnEnable()
		{
			B1NARYConsole.Log(name, "Initializing");
			_ = fadeController.FadeIn(0.5f);
		}
		private void OnDisable()
		{
			B1NARYConsole.Log(name, "De-Initializing");
			_ = fadeController.FadeOut(0.5f);
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
			StopSpeaking(false).Wait();
		}

		/// <summary>
		/// A <see cref="Task"/> that reveals <paramref name="speech"/> over time.
		/// </summary>
		/// <param name="speech">The speech to reveal.</param>
		/// <param name="token">The token to stop it.</param>
		/// <param name="lengthPerChar">Milliseconds to wait per character.</param>
		/// <returns> <paramref name="speech"/>. </returns>
		private Task<string> Speaking(string speech, CancellationToken token, int lengthPerChar = 30)
		{
			int startingLength = NewLine().Length; // Starts right after NewLine();
			speech = (NewLine() + speech).Trim();
			isWaitingForUserInput = false;
			bool inTag = false;
			for (int i = startingLength; i < speech.Length && !token.IsCancellationRequested; i++)
			{
				char character = speech[i];
				if (inTag)
				{
					if (character == '>')
						inTag = false;
					continue;
				}
				if (character == '<')
				{
					inTag = true;
					continue;
				}
				CurrentText += character;
				Task.Delay(lengthPerChar);
			}
			//text finished
			isWaitingForUserInput = true;
			return Task.FromResult(speech);
		}

	}
}