namespace B1NARY.CharacterManagement
{
	using B1NARY.DataPersistence;
	using B1NARY.DesignPatterns;
	using B1NARY.Scripting;
	using B1NARY.UI;
	using Codice.Client.Common;
	using Live2D.Cubism.Framework.Expression;
	using System;
	using System.Collections;
	using System.Linq;
	using UnityEngine;

	[RequireComponent(typeof(Animator))]
	public class Live2DCharacterController : MonoBehaviour, ICharacterController
	{
		bool ICharacterController.EmptyCharacter => false;
		public static string[] ToNameArray(CubismExpressionList list)
		{
			var expressions = new string[list.CubismExpressionObjects.Length];
			for (int i = 0; i < expressions.Length; i++)
			{
				string expression = list.CubismExpressionObjects[i].name;
				expressions[i] = expression.Remove(expression.LastIndexOf('.'));
			}
			return expressions;
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
		public string[] Expressions
		{
			get
			{
				if (m_expressions is null)
				{
					if (expressionController != null && expressionController.ExpressionsList != null)
						m_expressions = ToNameArray(expressionController.ExpressionsList);
					else
						m_expressions = Array.Empty<string>();
				}
				return m_expressions;
			}
		}
		private string[] m_expressions;
		public VoiceActorHandler VoiceData { get; private set; }
		public float selectedSizeIncreaseMultiplier = 1.1f;
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
			get => Expressions[expressionController.CurrentExpressionIndex];
			set
			{
				int expressionIndex = Array.IndexOf(Expressions, value);
				if (expressionIndex == -1)
				{
					Debug.LogException(new IndexOutOfRangeException($"'{value}' " +
						$"is not an expression listed in the expressions of {name}!\n"
						+ $"All options: {string.Join(",\n", Expressions)}"), gameObject);
					return;
				}
				expressionController.CurrentExpressionIndex = expressionIndex;
			}
		}

		public string CharacterName { get; set; }
		string ICharacterController.GameObjectName => gameObject.name;

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
		private CoroutineWrapper PositionChanger;

		public bool Selected
		{
			get => Selected;
			set
			{
				if (m_selected == value)
					return;
				m_selected = value;
				// Custom epsilon
				if (Math.Abs(selectedSizeIncreaseMultiplier - 1f) < 0.005f)
					return;
				Transform transform = this.transform;
				Vector3 currentScale = transform.localScale;
				float CurrentScaleMag() => currentScale.magnitude;
				Vector3 toScale = m_selected ? currentScale * selectedSizeIncreaseMultiplier : currentScale / selectedSizeIncreaseMultiplier;
				float toScaleMag = toScale.magnitude;
				if (!CoroutineWrapper.IsNotRunningOrNull(SizerSelection))
					SizerSelection.Stop();
				SizerSelection = new CoroutineWrapper(this, SmoothSizeChanger());
				SizerSelection.AfterActions += (mono) =>
				{
					Vector3 totalDiff = currentScale - toScale;
					transform.localScale += totalDiff;
				};
				SizerSelection.Start();
				IEnumerator SmoothSizeChanger()
				{
					float acceptablePoint = 0.005f;
					float velocity = 0f;
					while (Math.Abs(CurrentScaleMag() - toScaleMag) > acceptablePoint)
					{
						float newMag = Mathf.SmoothDamp(CurrentScaleMag(), toScaleMag, ref velocity, 0.25f);
						Vector3 newScale = currentScale.normalized * newMag;
						Vector3 scaleMagDiff = currentScale - newScale;
						currentScale = newScale;
						transform.localScale = currentScale + scaleMagDiff;
						yield return new WaitForEndOfFrame();
					}
				}
			}
		}
		private bool m_selected = false;
		private CoroutineWrapper SizerSelection;

		public void SetPositionOverTime(float newXPosition, float time)
		{
			if (!CoroutineWrapper.IsNotRunningOrNull(PositionChanger))
				PositionChanger.Stop();
			PositionChanger = new CoroutineWrapper(this, SmoothPosChanger());
			PositionChanger.AfterActions += (mono) => HorizontalPosition = newXPosition;
			PositionChanger.Start();
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

		protected virtual void Awake()
		{
			animator = GetComponent<Animator>();
			VoiceData = gameObject.AddComponent<VoiceActorHandler>();
			if (string.IsNullOrEmpty(CharacterName))
				CharacterName = gameObject.name;
		}

		public void SayLine(ScriptLine line)
		{
			DialogueSystem.Instance.Say(line.RawLine);
			VoiceData.Play(line);
		}

		CharacterSnapshot ICharacterController.Serialize()
		{
			CharacterSnapshot snapshot = new CharacterSnapshot(this);
			return snapshot;
		}
		void ICharacterController.Deserialize(CharacterSnapshot snapshot)
		{
			ICharacterController thisInterface = this;
			thisInterface.CurrentExpression = snapshot.expression;
			thisInterface.CharacterName = snapshot.name;
			thisInterface.Selected = snapshot.selected;
			thisInterface.CurrentAnimation = snapshot.animation;
			thisInterface.HorizontalPosition = snapshot.horizontalPosition;
		}
	}
}

#if UNITY_EDITOR
namespace B1NARY.CharacterManagement.Editor
{
	using UnityEditor;

	[CustomEditor(typeof(Live2DCharacterController))]
	public class CharacterScriptEditor : ControllerEditor
	{

	}
}
#endif