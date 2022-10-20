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
	using TMPro;
	using B1NARY.Scripting.Experimental;

	public class DialogueSystem : Singleton<DialogueSystem>
	{
		public static void InitializeSystem(DialogueSystem systemComponent)
		{
			if (!systemComponent.gameObject.activeSelf)
				systemComponent.gameObject.SetActive(true);
			systemComponent.enabled = true;
		}
		public static IEnumerable<KeyValuePair<string, Delegate>> Commands = new Dictionary<string, Delegate>()
		{
			["textspeed"] = (Action<string>)(speedRaw =>
			{
				Instance.ticksPerChar = int.Parse(speedRaw);
			}),
			/* There is already a system in ScriptHandler that handles more of it.
			["additive"] = (Action<string>)(str =>
			{
				if (ScriptDocument.enabledHashset.Contains(str))
					Instance.additiveTextEnabled = true;
				else if (ScriptDocument.disabledHashset.Contains(str))
					Instance.additiveTextEnabled = false;
				else 
					throw new Exception();
			}),*/
		};

		public int ticksPerChar = 30;
		/// <summary>
		/// If the current dialogue should be added instead of skipping to a new
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
		/// A property that directly points to the text box of <see cref="Text"/>
		/// </summary>
		public string CurrentSpeaker
		{
			get => speakerBox.text;
			set => speakerBox.text = value;
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
		private bool IsAprilFools;
		private string NewLine()
		{
			if (AdditiveTextEnabled)
				return CurrentText + ' ';
			return string.Empty;
		}

		private void Awake()
		{
			IsAprilFools = DateTime.Today == new DateTime(DateTime.Today.Year, 4, 1);
		}
		private void Start()
		{
			CurrentSpeaker = string.Empty;
			CurrentText = string.Empty;
		}

		private CoroutineWrapper speakCoroutine;

		public void Say(string message)
		{
			if (!IsAprilFools)
				if (!CoroutineWrapper.IsNotRunningOrNull(speakCoroutine))
					speakCoroutine.Dispose();
			speakCoroutine = new CoroutineWrapper(this, Speaking(message)).Start();
		}

		public void Say(string message, string speaker)
		{
			CurrentSpeaker = speaker;
			Say(message);
		}

		public void StopSpeaking(bool? setFinalResultToCurrentText)
		{
			if (!CoroutineWrapper.IsNotRunningOrNull(speakCoroutine))
				speakCoroutine.Dispose();
			if (setFinalResultToCurrentText != null)
				if (setFinalResultToCurrentText == true)
					CurrentText = FinalText;
				else
					CurrentText = string.Empty;
		}


		private IEnumerator Speaking(string speech)
		{
			CurrentText = NewLine();
			FinalText = NewLine() + speech;
			var tagsList = new List<Tag>();
			int tagsLength = 0;
			float seconds = ticksPerChar / 1000f;
			for (int i = 0; i < speech.Length; i++)
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
					var currentTag = new Tag(tag.ToString());
					// Behaviour among the tag.
					if (currentTag.disableTag)
					{
						tagsList.Remove(currentTag.Opposite);
						tagsLength -= currentTag.TotalLength;
					}
					else
					{
						tagsLength += currentTag.Opposite.TotalLength;
						tagsList.Add(currentTag);
					}

					continue;
				}
				CurrentText = CurrentText.Insert(CurrentText.Length - tagsLength, speech[i].ToString());
				yield return new WaitForSeconds(seconds);
			}
		}
	}

	public struct Tag
	{
		public int TagLength => tagName.Length;
		public int TotalLength => ToString().Length;
		public readonly string tagName;
		public readonly bool disableTag;
		public Tag(string rawTag)
		{
			tagName = rawTag.Trim('<', '>', '/');
			disableTag = rawTag[1] == '/';
		}
		private Tag(string tag, bool disableTag)
		{
			tagName = tag;
			this.disableTag = disableTag;
		}
		public bool Equals(Tag tag, bool explicitType)
			=> (tag.tagName == tagName) && (explicitType ? disableTag == tag.disableTag : true);
		/// <summary>
		/// Simply returns an opposite <see cref="Tag"/>, doesn't affect the original.
		/// </summary>
		public Tag Opposite => new Tag(tagName, !disableTag);
		public override bool Equals(object obj)
		{
			if (obj is Tag tag)
				return Equals(tag, true);
			return base.Equals(obj);
		}
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
		public override string ToString() => $"<{(disableTag ? "/" : "")}{tagName}>";
	}
}