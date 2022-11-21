namespace B1NARY.CharacterManagement
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
		public string CurrentAnimation
		{
			get
			{
				return animator.GetCurrentAnimatorClipInfo(0).Single().clip.name;
			}
			set
			{
				value = value.Trim();
				try
				{
					animator.SetTrigger(value);
				}
				catch (NullReferenceException ex)
				{
					Debug.LogError($"BackgroundHandler '{value}' is not found in animation list"
						+ $"of character '{name}' \n{ex}");
				}
			}
		}

		public string CurrentExpression
		{
			get => expressionController.ExpressionsList.CubismExpressionObjects[expressionController.CurrentExpressionIndex].name;
			set
			{
				string[] cubismExpressions = expressionController.ExpressionsList
					.CubismExpressionObjects.Select(data => data.name).ToArray();
				int expressionIndex = Array.IndexOf(cubismExpressions, value);
				if (expressionIndex == -1)
				{
					Debug.LogException(new IndexOutOfRangeException($"'{value}' " +
						$"is not an expression listed in the expressions of {name}!"), gameObject);
					return;
				}
				expressionController.CurrentExpressionIndex = expressionIndex;
			}
		}

		public string CharacterName { get => gameObject.name; set => gameObject.name = value; }
		string ICharacterController.OldCharacterName { get; set; }

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

		private void OnEnable()
		{
			((ICharacterController)this).OldCharacterName = CharacterName;
		}
		private void OnDisable()
		{
			CharacterName = ((ICharacterController)this).OldCharacterName;
		}
		public void SayLine(ScriptLine line)
		{
			DialogueSystem.Instance.Say(line.lineData);
			VoiceData.Play(line);
		}
	}
}