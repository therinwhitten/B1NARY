namespace B1NARY.CharacterManagement
{
	using B1NARY.Scripting;
	using System.Collections.Generic;
	using UnityEngine;

	public interface IActor : IVoice
	{
		CharacterNames CharacterNames { get; }
		string GameObjectName { get; }
		void SayLine(ScriptLine line);

		Vector2 ScreenPosition { get; set; }
		void SetPositionOverTime(float xCoord, float time);
		void SetPositionOverTime(Vector2 newPos, float time);

		string CurrentAnimation { get; set; }
		/// <summary>
		/// The character's current expression. <see langword="null"/> if it has
		/// no current expression or unable to display any as it is invisible.
		/// </summary>
		string CurrentExpression { get; set; }
		bool Selected { get; set; }
		string CharacterTypeKey { get; }
		ActorSnapshot Serialize();
		void Deserialize(ActorSnapshot snapshot);
	}
	public interface IFollowable
	{
		Transform FollowCubeParent { get; }
	}
	public interface IVoice
	{
		IReadOnlyDictionary<int, VoiceActorHandler> Mouths { get; }
		int CurrentMouth { get; set; }
		void PlayClip(AudioClip clip, int mouth = -1);
		void Stop();
	}
}