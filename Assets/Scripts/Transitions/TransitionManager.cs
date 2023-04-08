namespace B1NARY
{
	using System;
	using System.Threading.Tasks;
	using UnityEngine;
	using UnityEngine.UI;
	using UnityEngine.Video;
	using UnityEngine.SceneManagement;
	using System.Linq;
	using System.Threading;
	using B1NARY.DesignPatterns;
	using System.Collections;
	using B1NARY.UI;
	using System.Collections.Generic;
	using B1NARY.Scripting;

	public class TransitionManager : Singleton<TransitionManager>
	{
		public static readonly CommandArray Commands = new CommandArray()
		{
			["changebg"] = (Action<string>)(backgroundName =>
			{
				InstanceOrDefault.SetNewStaticBackground(backgroundName);
				InstanceOrDefault.SetNewAnimatedBackground("Backgrounds/" + backgroundName);
			}),
			["loopbg"] = (Action<string>)(str =>
			{
				if (ScriptDocument.enabledHashset.Contains(str))
					InstanceOrDefault.LoopingAnimBG = true;
				else if (ScriptDocument.disabledHashset.Contains(str))
					InstanceOrDefault.LoopingAnimBG = false;
				else throw new ArgumentException($"{str} is not a valid " +
					$"argument for loopbg!");
			}),
			["playbg"] = (Action<string>)(backgroundName =>
			{
				InstanceOrDefault.SetNewAnimatedBackground("Backgrounds/" + backgroundName);
			}),
			["queuebg"] = (Action<string>)(backgroundName =>
			{
				InstanceOrDefault.AddQueueBackground("Backgrounds/" + backgroundName);
				InstanceOrDefault.LoopingAnimBG = false;
			}),
			["queuebgwait"] = (Action<string>)(length =>
			{
				InstanceOrDefault.queueWait = float.Parse(length);
			}),
			["queueloopbg"] = (Action<string>)(str =>
			{
				InstanceOrDefault.queuedActions.Enqueue((Action)(() =>
				{
					if (ScriptDocument.enabledHashset.Contains(str))
						InstanceOrDefault.LoopingAnimBG = true;
					else if (ScriptDocument.disabledHashset.Contains(str))
						InstanceOrDefault.LoopingAnimBG = false;
					else throw new ArgumentException($"{str} is not a valid " +
						$"argument for loopbg!");
				}));
			}),
		};

		[SerializeField] private string BGCanvasName = "BG-Canvas";
		//private BackgroundHandler m_backgroundHandler;
		//public BackgroundHandler Backgrounds => m_backgroundHandler;

		protected override void SingletonAwake()
		{
			UpdateBackgroundReferences();
			SceneManager.InstanceOrDefault.SwitchedScenes.AddPersistentListener(UpdateBackgroundReferences);
			void UpdateBackgroundReferences()
			{
				var obj = GameObject.Find(BGCanvasName);
				if (obj == null)
					throw new NullReferenceException(BGCanvasName);
				m_staticBackground = obj.GetComponentInChildren<Image>();
				m_animatedBackground = obj.GetComponentInChildren<VideoPlayer>();
			}
		}
		private float queueWait = 0f;
		protected virtual void LateUpdate()
		{
			if (m_animatedBackground == null)
				return;
			if (m_animatedBackground.isLooping || m_animatedBackground.isPlaying || !m_animatedBackground.isPaused)
				return;
			if (queueWait > 0f)
			{
				queueWait -= Time.deltaTime;
				return;
			}
			while (queuedActions.Count > 0 && !(queuedActions.Peek() is VideoClip))
			{
				object current = queuedActions.Dequeue();
				if (current is Action action)
					action.Invoke();
			}
			if (queuedActions.Count > 0 && queuedActions.Dequeue() is VideoClip clip)
				AnimatedBackground = clip;
		}

		public Sprite StaticBackground
		{
			get => m_staticBackground.sprite;
			set => m_staticBackground.sprite = value;
		}
		private Image m_staticBackground;
		public void SetNewStaticBackground(string resourcesPath)
			=> StaticBackground = Resources.Load<Sprite>(resourcesPath);

		public VideoClip AnimatedBackground
		{
			get => m_animatedBackground.clip;
			set
			{
				m_animatedBackground.Stop();
				m_animatedBackground.clip = value;
				m_animatedBackground.Play();
			}
		}
		private VideoPlayer m_animatedBackground;
		public bool LoopingAnimBG
		{
			get => m_animatedBackground.isLooping;
			set => m_animatedBackground.isLooping = value;
		}
		public void SetNewAnimatedBackground(string resourcesPath)
		{
			var clip = Resources.Load<VideoClip>(resourcesPath);
			if (clip == null)
				throw new InvalidOperationException($"'{resourcesPath}' animated background does not exist!");
			AnimatedBackground = clip;
			queuedActions.Clear();
			queueWait = 0f;
		}
		private Queue<object> queuedActions = new Queue<object>();
		public void AddQueueBackground(string resourcesPath)
		{
			var clip = Resources.Load<VideoClip>(resourcesPath);
			if (clip == null)
				throw new InvalidOperationException($"'{resourcesPath}' animated background does not exist!");
			queuedActions.Enqueue(clip);
		}
	}
}
#if UNITY_EDITOR
namespace B1NARY.Editor
{
	using System;
	using UnityEditor;

	[CustomEditor(typeof(TransitionManager)), Obsolete]
	public class TransitionHandlerEditor : Editor
	{
		private TransitionManager m_Handler;
		public TransitionManager TransitionHandler
		{
			get
			{
				if (m_Handler == null)
					m_Handler = (TransitionManager)target;
				return m_Handler;
			}
		}

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
			/*
			SerializedObject serializedObject = new SerializedObject(TransitionManager);
			serializedObject.Update();

			// Transitions
			EditorGUILayout.LabelField("Transitions", EditorStyles.boldLabel);
			EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(TransitionManager.transitionShader)));
			TransitionManager.useTransitionValue = EditorGUILayout.Toggle("Use Transition Value", TransitionManager.useTransitionValue);
			if (TransitionManager.transitionShader != null && TransitionManager.useTransitionValue)
				TransitionManager.fadePercentageName = EditorGUILayout
					.DelayedTextField("Fade Percentage Name", TransitionManager.fadePercentageName);
			EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(TransitionManager.textureIn)));
			EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(TransitionManager.textureOut)));

			// Backgrounds
			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Backgrounds", EditorStyles.boldLabel);
			TransitionManager.backgroundCanvasName = EditorGUILayout.DelayedTextField("Background Canvas Name", TransitionManager.backgroundCanvasName);
			TransitionManager.AutomaticallyAssignAnimatedBG = EditorGUILayout.ToggleLeft("Automatically Assign Animated Background", TransitionManager.AutomaticallyAssignAnimatedBG);

			serializedObject.ApplyModifiedProperties();
			*/
		}
	}
}
#endif