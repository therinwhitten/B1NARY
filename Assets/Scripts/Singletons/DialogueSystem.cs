using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;


public class DialogueSystem : Singleton<DialogueSystem>
{

    public string currentSpeaker = "";
    public ELEMENTS elements;

    public bool additiveTextEnabled = false;

    public int textFontSize = 30;

    public int nameTextFontSize = 30;
    // Use this for initialization
    void Awake()
    {
        // initialize();
    }
    public override void initialize()
    {
        elements.speechPanel = GameObject.Find("Panel-Speech");
        elements.speakerNameText = GameObject.Find("SpeakerName").GetComponent<Text>();
        elements.speechText = GameObject.Find("SpeechText").GetComponent<Text>();
        elements.speechText.fontSize = textFontSize;
        elements.speakerNameText.fontSize = nameTextFontSize;
    }
    /// <summary>
    /// Say something and show it on the speech box.
    /// </summary>
    public void Say(string speech)
    {
        StopSpeaking();

        speechText.text = targetSpeech;

        speaking = StartCoroutine(Speaking(speech));
    }

    public void SayRich(string speech)
    {
        StopSpeaking();

        speechText.text = targetSpeech;

        speaking = StartCoroutine(Speaking(speech, true));
    }

    public void StopSpeaking()
    {
        if (isSpeaking)
        {
            StopCoroutine(speaking);
        }
        speaking = null;
    }



    public bool isSpeaking { get { return speaking != null; } }
    [HideInInspector] public bool isWaitingForUserInput = false;

    [HideInInspector] public string targetSpeech = "";
    Coroutine speaking = null;
    IEnumerator Speaking(string speech, bool rich = false)
    {
        speechText.text = speechText.text.Trim();
        speechPanel.SetActive(true);
        targetSpeech = speech;

        if (!additiveTextEnabled)
            speechText.text = "";
        else
            targetSpeech = speechText.text + " " + targetSpeech;

        targetSpeech = targetSpeech.Trim();

        speakerNameText.text = currentSpeaker;

        isWaitingForUserInput = false;

        if (rich)
        {
            // if (additiveTextEnabled)
            //     targetSpeech = " " + speech;

            Stack<string> tags = new Stack<string>();
            string newTag = "";
            bool tagFound = false;
            string[] buffer = { "", "", "" };
            string initialText = speechText.text;
            for (int i = speechText.text.Length; i < targetSpeech.Length; i++)
            {
                char c = targetSpeech[i];
                if (tagFound)
                {
                    newTag += c;
                }

                if (c.Equals('<'))
                {
                    tagFound = true;
                    newTag += c;
                }

                if (c.Equals('>'))
                {
                    tagFound = false;
                    // if we found the closing tag
                    // if stack is empty simply push the new tag found onto it
                    if (tags.Count == 0 && !newTag.Contains('/'))
                    {
                        tags.Push(newTag);
                        initialText += buffer[0] + buffer[1] + buffer[2];

                        buffer[0] = newTag;
                        buffer[1] = "";
                        buffer[2] = getClosingTag(newTag);
                    }
                    else //check to see if it's a closing tag
                    {
                        // if yes then pop the tag off the stack
                        if (newTag.Contains('/'))
                        {
                            tags.Pop();

                            // remove tags from buffer
                            initialText += buffer[0] + buffer[1];
                            buffer[0] = "";
                            buffer[1] = "";

                            buffer[2].Trim();
                            ArrayList bufferWords = new ArrayList(buffer[2].Split(' '));
                            initialText += bufferWords[0];
                            bufferWords.RemoveAt(0);
                            buffer[2] = System.String.Join(null, bufferWords.Cast<string>());
                        }
                        else //push it on top of the last one
                        {
                            tags.Push(newTag);
                            buffer[0] = buffer[0] + buffer[1] + newTag;
                            buffer[2] = getClosingTag(newTag) + ' ' + buffer[2];
                            buffer[1] = "";
                        }
                    }
                    newTag = "";
                    continue;
                }
                if (!tagFound)
                {
                    buffer[1] += c;
                    speechText.text = initialText + buffer[0] + buffer[1] + buffer[2];
                    yield return new WaitForEndOfFrame();
                }
            }
        }
        else while (speechText.text != targetSpeech)
            {
                speechText.text += targetSpeech[speechText.text.Length];
                yield return new WaitForEndOfFrame();
            }

        //text finished
        isWaitingForUserInput = true;
        while (isWaitingForUserInput)
            yield return new WaitForEndOfFrame();

        StopSpeaking();
    }

    private string getClosingTag(string tag)
    {
        string closingTag = "</" + tag.Trim('<', '>') + ">";

        if (tag.Contains("="))
        {
            closingTag = closingTag.Split('=')[0] + ">";
        }

        return closingTag;
    }

    [System.Serializable]
    public class ELEMENTS
    {
        /// <summary>
        /// The main panel containing all dialogue related elements on the UI
        /// </summary>
        public GameObject speechPanel;
        public Text speakerNameText;
        public Text speechText;
    }
    public GameObject speechPanel { get { return elements.speechPanel; } }
    public Text speakerNameText { get { return elements.speakerNameText; } }
    public Text speechText { get { return elements.speechText; } }
}