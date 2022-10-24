namespace B1NARY
{
	using B1NARY.Audio;
	using B1NARY.Scripting;
	
	public interface ICharacterController
	{
		string CharacterName { get; set; }
		VoiceActorHandler VoiceData { get; }
		void SayLine(ScriptLine line);

		float HorizontalPosition { get; set; }
		void SetPositionOverTime(float xCoord, float time);

		string CurrentAnimation { get; }
		void PlayAnimation(string animationName);
		string CurrentExpression { get; }
		void ChangeExpression(string expressionName);
	}
}