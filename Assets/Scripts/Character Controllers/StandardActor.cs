namespace B1NARY.CharacterManagement
{
	using B1NARY.Scripting;
	using B1NARY.UI;
	using System;
	using System.Collections;
	using System.Linq;
	using UnityEngine;

	public class StandardActor : CachedMonobehaviour, IActor, IFollowable
	{
		public const string CHARACTER_KEY = "StandardCharacter"; 
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


		public string CharacterName { get; set; }
		string IActor.GameObjectName => gameObject.name;

		public Animator animator;
		public VoiceActorHandler VoiceData { get; private set; }

		public float HorizontalPosition
		{
			get => GetComponent<RectTransform>().anchorMin.x;
			set
			{
				GetComponent<RectTransform>().anchorMin = new Vector2(value, GetComponent<RectTransform>().anchorMin.y);
				GetComponent<RectTransform>().anchorMax = new Vector2(value, GetComponent<RectTransform>().anchorMax.y);
			}
		}

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
		string IActor.CurrentExpression { get => CurrentAnimation; set => CurrentAnimation = value; }

		public float selectedSizeIncreaseMultiplier = 1.1f;
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

		public Transform FollowCubeParent { get; set; }

		private bool m_selected = false;
		private CoroutineWrapper SizerSelection;


		protected virtual void Awake()
		{
			VoiceData = gameObject.AddComponent<VoiceActorHandler>();
			if (string.IsNullOrEmpty(CharacterName))
				CharacterName = gameObject.name;
		}

		public void SayLine(ScriptLine line)
		{
			DialogueSystem.Instance.Say(line.RawLine);
			VoiceData.Play(line);
		}

		void IActor.SetPositionOverTime(float xCoord, float time)
		{
			throw new NotImplementedException();
		}

		ActorSnapshot IActor.Serialize()
		{
			var snapshot = new ActorSnapshot(this) { expression = null };
			return snapshot;
		}
		void IActor.Deserialize(ActorSnapshot snapshot)
		{
			IActor thisInterface = this;
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
	using B1NARY.Editor;
	using UnityEditor;
	using UnityEngine;

	[CustomEditor(typeof(StandardActor))]
	public class StandardCharacterControllerEditor : ControllerEditor
	{
		public override void OnInspectorGUI()
		{
			StandardActor controller = (StandardActor)target;
			controller.animator = DirtyAuto.Field(controller, new GUIContent("Animator"), controller.animator, true);
			controller.FollowCubeParent = DirtyAuto.Field(controller, new GUIContent("Head Location"), controller.FollowCubeParent, true);
			EditorGUILayout.Space();
			base.OnInspectorGUI();
		}
	}
}
#endif