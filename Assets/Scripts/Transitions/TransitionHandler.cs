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

	[RequireComponent(typeof(CanvasGroup)), Obsolete]
	public class TransitionHandler : SingletonAlt<TransitionHandler>
	{
		public enum TransitionStatus : byte
		{
			FadedIn,
			FadedOut,
		}
		//[SerializeField] private string transitionName = "Transition";
		[SerializeField] private string BGCanvasName = "BG-Canvas";
		[SerializeField] private GameObject[] transitions;
		[SerializeField] private bool showTransitionOnFirstLoad = true;
		private BackgroundHandler m_backgroundHandler;
		public BackgroundHandler Backgrounds => m_backgroundHandler;
		private CanvasGroup canvasGroup;


		protected override void SingletonAwake()
		{
			canvasGroup = GetComponent<CanvasGroup>();
			PerScene();
		}
		private void Start()
		{
			if (showTransitionOnFirstLoad)
			{
				Show();
				FadeOut(0).FreeBlockPath();
			}
		}
		private void PerScene(string sceneName = "")
		{
			m_backgroundHandler = new BackgroundHandler(BGCanvasName);
			SceneManager.SwitchedScenes += PerScene;
		}


		public (Animator animator, TransitionSlave transitionSlave) GetAndVerifyAnimator(GameObject transitionObject)
		{
			if (transitionObject == null)
				throw new NullReferenceException($"{nameof(transitionObject)} doesn't exist!");
			var gameObject = Instantiate(transitionObject, this.gameObject.transform);
			gameObject.SetActive(true);
			var animator = gameObject.GetComponentWithChildren<Animator>();
			var transitionSlave = gameObject.GetComponentWithChildren<TransitionSlave>();
			return (animator, transitionSlave);
		}

		public async Task FadeIn(int index, float fadeMultiplier = 1f)
		{
			var (animator, transitionSlave) = GetAndVerifyAnimator(transitions[index]);
			Show();
			animator.speed = fadeMultiplier;
			await transitionSlave.StartAnimation();
		}
		public async Task FadeOut(int index, float fadeMultiplier = 1f)
		{
			var (animator, transitionSlave) = GetAndVerifyAnimator(transitions[index]);
			Show();
			animator.speed = fadeMultiplier;
			await transitionSlave.StopAnimation();
			Hide();
		}

		private void Hide()
		{
			canvasGroup.alpha = 0f;
			canvasGroup.blocksRaycasts = false;
			canvasGroup.interactable = false;
		}
		private void Show()
		{
			canvasGroup.alpha = 1f;
			canvasGroup.blocksRaycasts = true;
			canvasGroup.interactable = true;
		}


		private void OnDestroy()
		{

			SceneManager.SwitchedScenes -= PerScene;
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