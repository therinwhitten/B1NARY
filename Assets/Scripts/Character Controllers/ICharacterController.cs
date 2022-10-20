namespace B1NARY
{
	using B1NARY.Scripting.Experimental;
	
	public interface ICharacterController
	{
		string CharacterName { get; set; }
		void SayLine(ScriptLine line);

		void SetPosition(float xCoord);
		void SetPositionOverTime(float xCoord, float time, bool smooth);

		string CurrentAnimation { get; }
		void PlayAnimation(string animationName);
		string CurrentExpression { get; }
		void ChangeExpression(string expressionName);
	}
}