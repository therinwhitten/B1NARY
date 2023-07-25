namespace B1NARY.CharacterManagement
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

	/// <summary>
	/// A actor that is made in runtime. Doesn't have any textures, but still 
	/// technically a character with a voice ingame.
	/// </summary>
	[DisallowMultipleComponent]
	public class EmptyActor : MonoBehaviour, IActor
	{
		public const string CHARACTER_KEY = "Empty";
		string IActor.CharacterTypeKey => CHARACTER_KEY;
		private static readonly LinkedList<EmptyActor> allActors = new();

		[RuntimeInitializeOnLoadMethod]
		private static void Constructor()
		{
			ActorSnapshot.snapshot.Add(CHARACTER_KEY, Create);
		}

		public static Character Create(ActorSnapshot snapshot)
		{
			Character character = AddTo(CharacterManager.Instance);
			character.controller.Deserialize(snapshot);
			return character;
		}
		public static Character AddTo(CharacterManager characterManager)
		{
			var gameObject = new GameObject($"Empty Actor {allActors.Count}");
			gameObject.transform.SetParent(characterManager.Transform);
			EmptyActor controller = gameObject.AddComponent<EmptyActor>();
			characterManager.AddNewCharacter(gameObject, out Character character);
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
			allActors.AddLast(this);
			voices.Add(gameObject.AddComponent<VoiceActorHandler>());
			if (string.IsNullOrEmpty(CharacterNames.CurrentName))
				CharacterNames.CurrentName = gameObject.name;
		}
		private void OnDestroy()
		{
			allActors.Remove(this);
		}
		public CharacterNames CharacterNames { get; private set; } = new CharacterNames();
		string IActor.GameObjectName => gameObject.name;
		public void SayLine(ScriptLine line)
		{
			DialogueSystem.Instance.Say(line.RawLine);
			AudioClip voiceLine = VoiceActorHandler.GetVoiceLine(line.Index, ScriptHandler.Instance);
			(this as IVoice).PlayClip(voiceLine);
		}

		Vector2 IActor.ScreenPosition { get => new(0.5f, 0.5f); set { } }
		void IActor.SetPositionOverTime(float xCoord, float time)
		{
			
		}
		void IActor.SetPositionOverTime(Vector2 newPos, float time)
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
			CharacterNames = snapshot.characterNames;
			thisInterface.Selected = snapshot.selected;
			thisInterface.CurrentAnimation = snapshot.animation;
			thisInterface.ScreenPosition = snapshot.screenPosition;
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
		bool IActor.Selected { get; set; } = false;

		int IVoice.CurrentMouth { get; set; } = 0;
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