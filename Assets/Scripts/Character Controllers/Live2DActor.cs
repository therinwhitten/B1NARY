namespace B1NARY.CharacterManagement
{
	using B1NARY.DataPersistence;
	using B1NARY.DesignPatterns;
	using B1NARY.Scripting;
	using B1NARY.UI;
	using Live2D.Cubism.Framework.Expression;
	using Live2D.Cubism.Framework.MouthMovement;
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;
	using UnityEngine;

	[RequireComponent(typeof(Animator))]
	[DisallowMultipleComponent]
	public class Live2DActor : MonoBehaviour, IActor, IFollowable
	{
		public const string CHARACTER_KEY = "Live2DCharacter";
		string IActor.CharacterTypeKey => CHARACTER_KEY;
		[RuntimeInitializeOnLoadMethod]
		private static void Constructor()
		{
			ActorSnapshot.snapshot.Add(CHARACTER_KEY, Create);
		}

		public static Character Create(ActorSnapshot snapshot)
		{
			Character? nullableCharacter = CharacterManager.Instance.SummonCharacter(snapshot.gameObjectName);
			if (nullableCharacter == null)
				throw new NullReferenceException($"Failure to load {snapshot.gameObjectName} from data.");
			Character character = nullableCharacter.Value;
			character.ChangeCharacterName(snapshot.name);
			character.controller.HorizontalPosition = snapshot.horizontalPosition;
			character.controller.Deserialize(snapshot);
			return character;
		}
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
		public float selectedSizeIncreaseMultiplier = 1.1f;
		public CubismExpressionController expressionController;
		[field: SerializeField]
		public Transform FollowCubeParent { get; set; }
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
			get
			{
				if (expressionController != null && expressionController.CurrentExpressionIndex != -1)
					return Expressions[expressionController.CurrentExpressionIndex];
				return null;
			}
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
		string IActor.GameObjectName => gameObject.name;

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
			get => m_selected;
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

		IReadOnlyDictionary<int, VoiceActorHandler> IVoice.Mouths => mouths;
		private readonly Dictionary<int, VoiceActorHandler> mouths = new Dictionary<int, VoiceActorHandler>();
		int IVoice.CurrentMouth { get; set; } = 0;

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
			CubismAudioMouthInput[] voice = GetComponents<CubismAudioMouthInput>();
			for (int i = 0; i < voice.Length; i++)
			{
				VoiceActorHandler targetHandler = VoiceActorHandler.GetNewActor(voice[i].AudioInput);
				mouths.Add(voice[i].TargetMouth, targetHandler);
			}
			// Some Live2D characters just straight up doesn't have a mouth, so
			// - this is a quick fix for that.
			if (voice.Length <= 0)
			{
				VoiceActorHandler targetHandler = gameObject.AddComponent<VoiceActorHandler>();
				mouths.Add(0, targetHandler);
			}
			if (string.IsNullOrEmpty(CharacterName))
				CharacterName = gameObject.name;
		}

		public void SayLine(ScriptLine line)
		{
			DialogueSystem.Instance.Say(line.RawLine);
			(this as IVoice).PlayClip(VoiceActorHandler.GetVoiceLine(line.Index, ScriptHandler.Instance));
		}

		ActorSnapshot IActor.Serialize()
		{
			ActorSnapshot snapshot = new ActorSnapshot(this);
			return snapshot;
		}
		void IActor.Deserialize(ActorSnapshot snapshot)
		{
			IActor thisInterface = this;
			if (Expressions.Length > 0)
				if (!string.IsNullOrEmpty(snapshot.expression))
					thisInterface.CurrentExpression = snapshot.expression;
			thisInterface.CharacterName = snapshot.name;
			thisInterface.Selected = snapshot.selected;
			thisInterface.CurrentAnimation = snapshot.animation;
			thisInterface.HorizontalPosition = snapshot.horizontalPosition;
		}

		void IVoice.PlayClip(AudioClip clip, int mouth)
		{
			if (mouth <= -1)
				// No idea why i have to do this but ok
				mouth = (this as IVoice).CurrentMouth;
			mouths[mouth].Play(clip);
		}

		void IVoice.Stop()
		{
			using (var enumerator = mouths.GetEnumerator())
				while (enumerator.MoveNext())
					enumerator.Current.Value.Stop();
		}
	}
}

#if UNITY_EDITOR
namespace B1NARY.CharacterManagement.Editor
{
	using B1NARY.Editor;
	using UnityEditor;
	using UnityEngine;

	[CustomEditor(typeof(Live2DActor))]
	public class CharacterScriptEditor : ControllerEditor
	{
		public override void OnInspectorGUI()
		{
			Live2DActor controller = (Live2DActor)target;
			controller.FollowCubeParent = DirtyAuto.Field(controller, new GUIContent("Head Location"), controller.FollowCubeParent, true);
			EditorGUILayout.Space();
			base.OnInspectorGUI();
		}
	}
}
#endif