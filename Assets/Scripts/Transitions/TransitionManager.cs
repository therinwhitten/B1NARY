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
				InstanceOrDefault.SetNewAnimatedBackground(backgroundName);
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
				InstanceOrDefault.SetNewAnimatedBackground(backgroundName);
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
		}
	}
}