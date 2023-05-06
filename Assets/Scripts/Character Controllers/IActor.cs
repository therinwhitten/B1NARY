namespace B1NARY.CharacterManagement
{
	using B1NARY.Scripting;
	using UnityEngine;

	public interface IActor
	{
		string CharacterName { get; set; }
		string GameObjectName { get; }
		VoiceActorHandler VoiceData { get; }
		void SayLine(ScriptLine line);

		float HorizontalPosition { get; set; }
		void SetPositionOverTime(float xCoord, float time);

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
}