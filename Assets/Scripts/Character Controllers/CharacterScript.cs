namespace B1NARY.CharacterManagement
{
	using B1NARY.DataPersistence;
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
		public static string[] ToNameArray(CubismExpressionList list)
		{
			return list.CubismExpressionObjects
				.Select(param => param.name.Replace(".exp3", string.Empty)).ToArray();
		}

		private Animator animator;
		private RectTransform m_transform;
		public RectTransform Transform
		{
			get
			{
				if (m_transform == null)
					m_transform = GetComponent<RectTransform>();
				return m_transform;
			}
		}
		private string[] expressions;
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
			get => expressions[expressionController.CurrentExpressionIndex];
			set
			{
				int expressionIndex = Array.IndexOf(expressions, value);
				if (expressionIndex == -1)
				{
					Debug.LogException(new IndexOutOfRangeException($"'{value}' " +
						$"is not an expression listed in the expressions of {name}!\n"
						+ $"All options: {string.Join(",\n", expressions)}"), gameObject);
					return;
				}
				expressionController.CurrentExpressionIndex = expressionIndex;
			}
		}

		public string CharacterName { get => gameObject.name; set => gameObject.name = value; }
		string ICharacterController.OldCharacterName { get; set; }

		public Vector2 Position 
		{ 
			get => Transform.position;
			set => Transform.position = value; 
		}
		public float HorizontalPosition
		{
			get => Transform.anchorMin.x;
			set
			{
				Transform.anchorMin = new Vector2(value, Transform.anchorMin.y);
				Transform.anchorMax = new Vector2(value, Transform.anchorMax.y);
			}
		}

		bool ICharacterController.Selected 
		{
			get;
			set;
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
			}
		}

		protected override void MultitonAwake()
		{
			animator = GetComponent<Animator>();
			VoiceData = gameObject.AddComponent<VoiceActorHandler>();
			if (string.IsNullOrEmpty(CharacterName))
				CharacterName = gameObject.name;
			if (expressionController != null && expressionController.ExpressionsList != null)
				expressions = ToNameArray(expressionController.ExpressionsList);
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
			DialogueSystem.Instance.Say(line.RawLine);
			VoiceData.Play(line);
		}
	}
}

#if UNITY_EDITOR
namespace B1NARY.CharacterManagement.Editor
{
	using UnityEditor;

	[CustomEditor(typeof(CharacterScript))]
	public class CharacterScriptEditor : ControllerEditor
	{

	}
}
#endif