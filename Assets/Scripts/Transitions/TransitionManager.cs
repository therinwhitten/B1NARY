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
	using System.Globalization;

	/// <summary>
	/// A manager that handles the backgrounds, and allows to queue them to branch
	/// between non-looping videos.
	/// </summary>
	public class TransitionManager : Singleton<TransitionManager>
	{
		public static readonly CommandArray Commands = new()
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
				InstanceOrDefault.AddAnimatedQueueBackground("Backgrounds/" + backgroundName);
				InstanceOrDefault.LoopingAnimBG = false;
			}),
			//["queuebgwait"] = (Action<string>)(length =>
			//{
			//	InstanceOrDefault.queueWait = float.Parse(length);
			//}),
			["queueloopbg"] = (Action<string>)(str =>
			{
				InstanceOrDefault.Queued.Enqueue((Action)(() =>
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

		public Sprite StaticBackground
		{
			get => _staticBackground.sprite;
			set
			{
				_staticBackground.sprite = value;
				_animatedBackground.Stop();
				_animatedBackground.clip = null;
				ClearQueued();
			}
		}
		private Image _staticBackground;

		public bool SetNewStaticBackground(string resourcesPath)
		{
			Sprite sprite = Resources.Load<Sprite>(resourcesPath);
			if (sprite == null)
				return false;
			StaticBackground = sprite;
			return true;
		}

		public VideoClip AnimatedBackground 
		{ 
			get => _animatedBackground.clip; 
			set
			{
				_animatedBackground.Stop();
				_animatedBackground.clip = value;
				_animatedBackground.Play();
				ClearQueued();
			}
		}
		private VideoPlayer _animatedBackground;
		public bool LoopingAnimBG
		{
			get => _animatedBackground.isLooping;
			set => _animatedBackground.isLooping = value;
		}

		public bool SetNewAnimatedBackground(string resourcesPath)
		{
			var clip = Resources.Load<VideoClip>(resourcesPath);
			if (clip == null)
				return false;
			AnimatedBackground = clip;
			return true;
		}

		protected override void SingletonAwake()
		{
			UpdateBackgroundReferences();
			SceneManager.InstanceOrDefault.SwitchedScenes.AddPersistentListener(UpdateBackgroundReferences);
		}
		private void UpdateBackgroundReferences()
		{
			var obj = GameObject.Find(BGCanvasName);
			if (obj == null)
				throw new NullReferenceException(BGCanvasName);
			_staticBackground = obj.GetComponentInChildren<Image>();
			_animatedBackground = obj.GetComponentInChildren<VideoPlayer>();
		}


		private void LateUpdate()
		{
			QueueUpdate();
		}
		protected override void OnSingletonDestroy()
		{
			base.OnSingletonDestroy();
		}


		#region Queueing System
		private readonly Queue<Action> Queued = new();

		private void QueueUpdate()
		{
			if (Queued.Count <= 0)
				return;
			if (_animatedBackground.isPlaying)
				return;
			Queued.Dequeue().Invoke();
		}

		private void ClearQueued()
		{
			Queued.Clear();
		}


		public bool AddAnimatedQueueBackground(string resourcesPath)
		{
			var clip = Resources.Load<VideoClip>(resourcesPath);
			if (clip == null)
				return false;
			Queued.Enqueue(() => AnimatedBackground = clip);
			return true;
		}
		public bool AddStaticQueueBackground(string resourcesPath)
		{
			var clip = Resources.Load<Sprite>(resourcesPath);
			if (clip == null)
				return false;
			Queued.Enqueue(() => StaticBackground = clip);
			return true;
		}
		#endregion
	}
	/*
	public class TransitionManager : Singleton<TransitionManager>
	{
		public static readonly CommandArray Commands = new()
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
	*/
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