namespace B1NARY
{
	using B1NARY.Audio;
	using B1NARY.DesignPatterns;
	using B1NARY.Scripting.Experimental;
	using B1NARY.UI;
	using Live2D.Cubism.Framework.Expression;
	using System;
	using System.Collections;
	using System.Linq;
	using UnityEngine;

	[RequireComponent(typeof(Animator))]
	public sealed class CharacterScript : Multiton<CharacterScript>
	{
		private Animator animator;
		private Transform m_transform;
		public VoiceActorHandler VoiceActorHandler { get; private set; }
		public CubismExpressionController expressionController;
		public string CurrentAnimation => animator.GetCurrentAnimatorClipInfo(0).Single().clip.name;
		public string CurrentExpression => expressionController.ExpressionsList.CubismExpressionObjects[expressionController.CurrentExpressionIndex].name;
		public string CharacterName { get => gameObject.name; set => gameObject.name = value; }
		public Vector2 Position 
		{ 
			get => m_transform.position;
			set => m_transform.position = value; 
		}
		[Obsolete("Just set Position instead")]
		public void SetPosition(Vector2 newPosition)
			=> Position = newPosition;
		public void SetPosition(float xCoord)
			=> Position = new Vector2(xCoord, Position.y);
		public void SetPositionOverTime(Vector2 newPosition, float time, bool smooth)
		{
			if (smooth)
				StartCoroutine(SmoothPosChanger());
			else
				StartCoroutine(StaticPosChanger());
			IEnumerator SmoothPosChanger()
			{
				var acceptableRect = new Rect(newPosition, new Vector2(0.1f, 0.1f));
				Vector2 velocity = Vector2.zero;
				while (acceptableRect.Contains(m_transform.position) == false)
				{
					m_transform.position = Vector2.SmoothDamp(m_transform.position, newPosition, ref velocity, Time.deltaTime / time);
					yield return new WaitForEndOfFrame();
				}
				Debug.Log("Finished smooth transfer");
			}
			IEnumerator StaticPosChanger()
			{
				throw new NotImplementedException();
				Debug.Log("Finished static transfer");
			}
		}

		protected override void MultitonAwake()
		{
			m_transform = transform;
			animator = GetComponent<Animator>();
			VoiceActorHandler = new VoiceActorHandler(gameObject);
			if (string.IsNullOrEmpty(CharacterName))
				CharacterName = gameObject.name;
		}

		public void PlayAnimation(string animationName)
		{
			animationName = animationName.Trim(); 
			try
			{
				animator.SetTrigger(animationName);
			}
			catch (NullReferenceException ex)
			{
				Debug.LogError($"BackgroundHandler '{animationName}' is not found in animation list"
					+ $"of character '{name}' \n{ex}");
			}
		}
		public void ChangeExpression(string expressionName)
		{
			string[] cubismExpressions = expressionController.ExpressionsList
				.CubismExpressionObjects.Select(data => data.name).ToArray();
			int expressionIndex = Array.IndexOf(cubismExpressions, expressionName);
			if (expressionIndex == -1)
			{
				Debug.LogException(new IndexOutOfRangeException($"'{expressionName}' " +
					$"is not an expression listed in the expressions of {name}!"), gameObject);
				return;
			}
			expressionController.CurrentExpressionIndex = expressionIndex;
		}
		public void SayLine(ScriptLine line)
		{
			DialogueSystem.Instance.Say(line.lineData);
			VoiceActorHandler.Play(line);
		}
	}
}