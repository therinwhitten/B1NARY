using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;


public class DialogueSystem : MonoBehaviour
{
    public static DialogueSystem instance;

    public ELEMENTS elements;

    void Awake()
    {
        instance = this;
    }

    // Use this for initialization
    void Start()
    {
    }

    /// <summary>
    /// Say something and show it on the speech box.
    /// </summary>
    public void Say(string speech, string speaker = "")
    {
        StopSpeaking();

        speaking = StartCoroutine(Speaking(speech, false, speaker));
    }

    /// <summary>
    /// Say something to be added to what is already on the speech box.
    /// </summary>
    public void SayAdd(string speech, string speaker = "")
    {
        StopSpeaking();

        speechText.text = targetSpeech;

        speaking = StartCoroutine(Speaking(speech, true, speaker));
    }

    public void SayRich(string speech, string speaker = "")
    {
        StopSpeaking();

        speaking = StartCoroutine(Speaking(speech, false, speaker, true));
    }

    public void sayAddRich(string speech, string speaker = "")
    {
        StopSpeaking();

        speechText.text = targetSpeech;

        speaking = StartCoroutine(Speaking(speech, true, speaker, true));
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

    public string targetSpeech = "";
    Coroutine speaking = null;
    IEnumerator Speaking(string speech, bool additive, string speaker = "", bool rich = false)
    {
        speechPanel.SetActive(true);
        targetSpeech = speech;

        if (!additive)
            speechText.text = "";
        else
            targetSpeech = speechText.text + " " + targetSpeech;

        speakerNameText.text = DetermineSpeaker(speaker);//temporary

        isWaitingForUserInput = false;

        if (rich)
        {
            Stack<string> tags = new Stack<string>();
            string newTag = "";
            bool tagFound = false;
            string[] buffer = { "", "", "" };
            string initialText = speechText.text;
            foreach (char c in targetSpeech)
            {
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
                        initialText = buffer[0] + buffer[1] + buffer[2];

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
                            // ArrayList bufferWords = new ArrayList(buffer[0].Split(' '));
                            // bufferWords.RemoveAt(bufferWords.Count - 1);
                            // buffer[0] = System.String.Join(null, bufferWords.Cast<string>());
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
                    // Debug.Log(buffer[0] + buffer[1] + buffer[2]);
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

    string DetermineSpeaker(string s)
    {
        string retVal = speakerNameText.text;//default return is the current name
        if (s != speakerNameText.text && s != "")
            retVal = (s.ToLower().Contains("narrator")) ? "" : s;

        return retVal;
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