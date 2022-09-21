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
	using B1NARY.Scripting.Experimental;

	[RequireComponent(typeof(CanvasGroup))]
	public class TransitionManager : SingletonAlt<TransitionManager>
	{
		public static IReadOnlyDictionary<string, Delegate> TransitionDelegateCommands = new Dictionary<string, Delegate>()
		{
			["changebg"] = (Action<string>)(backgroundName =>
			{
				Instance.Backgrounds.SetNewStaticBackground(backgroundName);
				Instance.Backgrounds.SetNewAnimatedBackground(backgroundName);
			}),
			["loopbg"] = (Action<string>)(str =>
			{
				if (ScriptDocument.enabledHashset.Contains(str))
					Instance.Backgrounds.LoopingAnimBG = true;
				else if (ScriptDocument.disabledHashset.Contains(str))
					Instance.Backgrounds.LoopingAnimBG = false;
				else throw new ArgumentException($"{str} is not a valid " +
					$"argument for loopbg!");
			}),
			["playbg"] = (Action<string>)(backgroundName =>
			{
				TransitionManager.Instance.Backgrounds.SetNewAnimatedBackground(backgroundName);
			}),
		};

		[SerializeField] private string BGCanvasName = "BG-Canvas";
		[SerializeField] private GameObject[] transitions;
		//[SerializeField] private bool showTransitionOnFirstLoad = true;
		private BackgroundHandler m_backgroundHandler;
		public BackgroundHandler Backgrounds => m_backgroundHandler;


		protected override void SingletonAwake()
		{
			SceneManager.SwitchedScenes.AddPersistentListener(PerScene);
			PerScene();
		}
		private void PerScene()
		{
			m_backgroundHandler = new BackgroundHandler(BGCanvasName);
		}


		public TransitionObject GetTransitionObject(GameObject gameSchematic)
		{
			if (gameSchematic == null)
				throw new NullReferenceException($"{gameSchematic.name} doesn't exist!");
			var gameObject = Instantiate(gameSchematic, this.gameObject.transform);
			gameObject.SetActive(true);
			return gameObject.GetComponent<TransitionObject>();
		}

		public async Task SetToOpaque(int index, float fadeMultiplier = 1f)
		{
			TransitionObject transitionObject = GetTransitionObject(transitions[index]);
			await transitionObject.SetToOpaque(fadeMultiplier);
			Destroy(transitionObject.gameObject);
		}
		public async Task SetToTransparent(int index, float fadeMultiplier = 1f)
		{
			TransitionObject transitionObject = GetTransitionObject(transitions[index]);
			await transitionObject.SetToTransparent(fadeMultiplier);
			Destroy(transitionObject.gameObject);
		}



		private void OnDestroy()
		{
			SceneManager.SwitchedScenes.RemovePersistentListener(PerScene);
		}

		[Serializable]
		public class BackgroundHandler
		{
			public bool AutomaticallyAssignAnimatedBG = true;
			public (GameObject gameObject, Image staticBG, VideoPlayer animatedBG) BGCanvas { get; private set; }
			public bool LoopingAnimBG { get => BGCanvas.animatedBG.isLooping; set => BGCanvas.animatedBG.isLooping = value; }

			internal BackgroundHandler(string BGCanvasName)
			{
				var obj = GameObject.Find(BGCanvasName);
				if (obj == null)
					throw new NullReferenceException(BGCanvasName);
				BGCanvas = (obj, obj.GetComponentWithChildren<Image>(), obj.GetComponentWithChildren<VideoPlayer>());
			}

			public void SetNewStaticBackground(string resourcesPath)
				=> SetNewStaticBackground(Resources.Load<Sprite>(resourcesPath));
			public void SetNewStaticBackground(Sprite sprite) =>
				BGCanvas.staticBG.sprite = sprite;
			public void SetNewAnimatedBackground(string resourcesPath)
				=> SetNewAnimatedBackground(Resources.Load<VideoClip>(resourcesPath));
			public void SetNewAnimatedBackground(VideoClip videoClip)
			{
				BGCanvas.animatedBG.Stop();
				BGCanvas.animatedBG.clip = videoClip;
				BGCanvas.animatedBG.Play();
			}

			
		}
	}
}