namespace B1NARY
{
	using B1NARY.Audio;
	using B1NARY.DesignPatterns;
	using B1NARY.Scripting;
	using B1NARY.UI;
	using Live2D.Cubism.Framework.Expression;
	using System;
	using System.Collections;
	using System.Linq;
	using UnityEngine;

	[RequireComponent(typeof(Animator))]
	public sealed class CharacterScript : Multiton<CharacterScript>, ICharacterController
	{
		private Animator animator;
		private RectTransform m_transform;
		public VoiceActorHandler VoiceData { get; private set; }
		public CubismExpressionController expressionController;
		public string CurrentAnimation => animator.GetCurrentAnimatorClipInfo(0).Single().clip.name;
		public string CurrentExpression => expressionController.ExpressionsList.CubismExpressionObjects[expressionController.CurrentExpressionIndex].name;
		public string CharacterName { get => gameObject.name; set => gameObject.name = value; }
		public Vector2 Position 
		{ 
			get => m_transform.position;
			set => m_transform.position = value; 
		}
		public float HorizontalPosition
		{
			get => m_transform.anchorMin.x;
			set
			{
				m_transform.anchorMin = new Vector2(value, m_transform.anchorMin.y);
				m_transform.anchorMax = new Vector2(value, m_transform.anchorMax.y);
			}
		}
		public void SetPositionOverTime(float newXPosition, float time)
		{
			StartCoroutine(SmoothPosChanger());
			IEnumerator SmoothPosChanger()
			{
				float acceptablePoint = 0.005f;
				float velocity = 0f;
				while (Math.Abs(HorizontalPosition - newXPosition) > acceptablePoint)
				{
					HorizontalPosition = Mathf.SmoothDamp(HorizontalPosition, newXPosition, ref velocity, time);
					yield return new WaitForEndOfFrame();
				}
				Debug.Log("Finished smooth transfer");
			}
		}

		protected override void MultitonAwake()
		{
			m_transform = GetComponent<RectTransform>();
			animator = GetComponent<Animator>();
			VoiceData = gameObject.AddComponent<VoiceActorHandler>();
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
			VoiceData.Play(line);
		}
	}
}