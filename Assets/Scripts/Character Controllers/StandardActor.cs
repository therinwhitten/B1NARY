namespace B1NARY.CharacterManagement
{
	using B1NARY.Scripting;
	using B1NARY.UI;
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;
	using UnityEngine;

	/// <summary>
	/// 
	/// </summary>
	[DisallowMultipleComponent]
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
			(character.controller as StandardActor).CharacterNames = snapshot.characterNames;
			character.controller.ScreenPosition = snapshot.screenPosition;
			character.controller.Deserialize(snapshot);
			return character;
		}


		public CharacterNames CharacterNames { get; set; } = new CharacterNames();
		string IActor.GameObjectName => gameObject.name;

		public Animator animator;
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

		private CoroutineWrapper PositionChanger;
		public Vector2 ScreenPosition
		{
			get => Transform.anchorMin;
			set
			{
				Transform.anchorMin = value;
				Transform.anchorMax = value;
			}
		}

		private bool m_selected = false;
		private CoroutineWrapper SizerSelection;


		protected virtual void Awake()
		{
			animator = GetComponent<Animator>();
			AudioSource[] voice = GetComponents<AudioSource>();
			for (int i = 0; i < voice.Length; i++)
			{
				VoiceActorHandler targetHandler = VoiceActorHandler.GetNewActor(voice[i]);
				mouths.Add(i, targetHandler);
			}
			if (string.IsNullOrEmpty(CharacterNames.CurrentName))
				CharacterNames.CurrentName = gameObject.name;

			// Getting vertical and horizontal axis from a range to a point
			Vector2 anchorMin = Transform.anchorMin;
			anchorMin.x = (Transform.anchorMin.x + Transform.anchorMax.x) * 0.5f;
			anchorMin.y = (Transform.anchorMin.y + Transform.anchorMax.y) * 0.5f;
			Transform.anchorMin = anchorMin;
			Transform.anchorMax = anchorMin;
		}

		public void SayLine(ScriptLine line)
		{
			DialogueSystem.Instance.Say(line.RawLine);
			(this as IVoice).PlayClip(VoiceActorHandler.GetVoiceLine(line.Index, ScriptHandler.Instance));
		}


		ActorSnapshot IActor.Serialize()
		{
			var snapshot = new ActorSnapshot(this) { expression = null };
			return snapshot;
		}
		void IActor.Deserialize(ActorSnapshot snapshot)
		{
			IActor thisInterface = this;
			CharacterNames = snapshot.characterNames;
			thisInterface.Selected = snapshot.selected;
			thisInterface.CurrentAnimation = snapshot.animation;
			thisInterface.ScreenPosition = snapshot.screenPosition;
		}


		void IActor.SetPositionOverTime(Vector2 newPosition, float time)
		{
			if (!CoroutineWrapper.IsNotRunningOrNull(PositionChanger))
				PositionChanger.Stop();
			PositionChanger = new CoroutineWrapper(this, SmoothPosChanger());
			PositionChanger.AfterActions += (mono) => ScreenPosition = newPosition;
			PositionChanger.Start();
			IEnumerator SmoothPosChanger()
			{
				float acceptablePoint = 0.005f;
				Vector2 velocity = Vector2.zero;
				while (Math.Abs((ScreenPosition - newPosition).magnitude) > acceptablePoint)
				{
					ScreenPosition = Vector2.SmoothDamp(ScreenPosition, newPosition, ref velocity, time);
					yield return new WaitForEndOfFrame();
				}
			}
		}
		void IActor.SetPositionOverTime(float newXPosition, float time)
		{
			if (!CoroutineWrapper.IsNotRunningOrNull(PositionChanger))
				PositionChanger.Stop();
			PositionChanger = new CoroutineWrapper(this, SmoothPosChanger());
			PositionChanger.AfterActions += (mono) => ScreenPosition = new Vector2(newXPosition, ScreenPosition.y);
			PositionChanger.Start();
			IEnumerator SmoothPosChanger()
			{
				float acceptablePoint = 0.005f;
				float velocity = 0f;
				while (Math.Abs(ScreenPosition.x - newXPosition) > acceptablePoint)
				{
					ScreenPosition = new Vector2(Mathf.SmoothDamp(ScreenPosition.x, newXPosition, ref velocity, time), ScreenPosition.y);
					yield return new WaitForEndOfFrame();
				}
			}
		}

		private void Reset()
		{
			animator = GetComponent<Animator>();
		}




		void IVoice.PlayClip(AudioClip clip, int mouth)
		{
			if (mouths.Count <= 0)
			{
				VoiceActorHandler targetHandler = gameObject.AddComponent<VoiceActorHandler>();
				mouths.Add(0, targetHandler);
			}

			if (mouth <= -1)
				mouth = (this as IVoice).CurrentMouth;
			mouths[mouth].Play(clip);
		}

		void IVoice.Stop()
		{
			using var enumerator = mouths.GetEnumerator();
			while (enumerator.MoveNext())
				enumerator.Current.Value.Stop();
		}
		IReadOnlyDictionary<int, VoiceActorHandler> IVoice.Mouths => mouths;
		private readonly Dictionary<int, VoiceActorHandler> mouths = new();
		int IVoice.CurrentMouth { get; set; } = 0;
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
			controller.animator = DirtyAuto.Field(controller, new("Animator"), controller.animator, true);
			controller.FollowCubeParent = DirtyAuto.Field(controller, new("Head Location"), controller.FollowCubeParent, true);
			EditorGUILayout.Space();
			base.OnInspectorGUI();
		}
	}
}
#endif