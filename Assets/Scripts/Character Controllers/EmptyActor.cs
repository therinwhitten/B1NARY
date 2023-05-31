﻿namespace B1NARY.CharacterManagement
{
	using B1NARY.Audio;
	using B1NARY.Scripting;
	using B1NARY.UI;
	using Live2D.Cubism.Framework.MouthMovement;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using UnityEditor;
	using UnityEngine;

	public class EmptyActor : MonoBehaviour, IActor
	{
		public const string CHARACTER_KEY = "Empty";
		string IActor.CharacterTypeKey => CHARACTER_KEY;

		[RuntimeInitializeOnLoadMethod]
		private static void Constructor()
		{
			ActorSnapshot.snapshot.Add(CHARACTER_KEY, Create);
		}

		public static Character Create(ActorSnapshot snapshot)
		{
			Character character = AddTo(CharacterManager.Instance, snapshot.name);
			character.controller.Deserialize(snapshot);
			return character;
		}
		public static Character AddTo(CharacterManager characterManager, string name)
		{
			var gameObject = new GameObject(name);
			gameObject.transform.SetParent(characterManager.Transform);
			EmptyActor controller = gameObject.AddComponent<EmptyActor>();
			characterManager.AddCharacterToDictionary(gameObject, out Character character);
			return character;
		}
		public static (GameObject @object, EmptyActor emptyController) Instantiate(Transform parent, string name)
		{
			var gameObject = new GameObject(name);
			gameObject.transform.parent = parent;
			var emptyController = gameObject.AddComponent<EmptyActor>();
			return (gameObject, emptyController);
		}

		private void Awake()
		{
			voices.Add(gameObject.AddComponent<VoiceActorHandler>());
			if (string.IsNullOrEmpty(CharacterName))
				CharacterName = gameObject.name;
		}
		public string CharacterName { get; set; }
		string IActor.GameObjectName => gameObject.name;
		public void SayLine(ScriptLine line)
		{
			DialogueSystem.Instance.Say(line.RawLine);
			AudioClip voiceLine = VoiceActorHandler.GetVoiceLine(line.Index, ScriptHandler.Instance);
			(this as IVoice).PlayClip(voiceLine);
		}

		float IActor.HorizontalPosition { get => 0f; set { } }
		void IActor.SetPositionOverTime(float xCoord, float time)
		{
			
		}

		ActorSnapshot IActor.Serialize()
		{
			ActorSnapshot snapshot = new ActorSnapshot(this);
			return snapshot;
		}
		void IActor.Deserialize(ActorSnapshot snapshot)
		{
			IActor thisInterface = this;
			thisInterface.CurrentExpression = snapshot.expression;
			thisInterface.CharacterName = snapshot.name;
			thisInterface.Selected = snapshot.selected;
			thisInterface.CurrentAnimation = snapshot.animation;
			thisInterface.HorizontalPosition = snapshot.horizontalPosition;
		}

		void IVoice.PlayClip(AudioClip clip, int mouth)
		{
			if (mouth <= -1)
				// No idea why i have to do this but ok
				mouth = (this as IVoice).CurrentMouth;
			voices[mouth].Play(clip);
		}

		void IVoice.Stop()
		{
			for (int i = 0; i < voices.Count; i++)
				voices[i].Stop();
		}
		IReadOnlyDictionary<int, VoiceActorHandler> IVoice.Mouths
		{
			get
			{
				int i = -1;
				return voices.ToDictionary(item => { i++; return i; });
			}
		}
		private readonly List<VoiceActorHandler> voices = new List<VoiceActorHandler>();

		string IActor.CurrentAnimation
		{
			get => string.Empty;
			set { }
		}
		string IActor.CurrentExpression
		{
			get => string.Empty;
			set { }
		}
		bool IActor.Selected 
		{ 
			get => m_selected; 
			set 
			{
				if (m_selected == value)
					return;
				m_selected = value;

			}
		}

		int IVoice.CurrentMouth { get; set; } = 0;

		private bool m_selected = false;
	}
}

#if UNITY_EDITOR
namespace B1NARY.CharacterManagement.Editor
{
	using UnityEditor;

	[CustomEditor(typeof(EmptyActor))]
	public class EmptyControllerEditor : ControllerEditor
	{

	}
}
#endif