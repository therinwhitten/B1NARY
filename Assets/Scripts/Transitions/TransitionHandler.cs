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
		private BackgroundHandler m_backgroundHandler;
		public BackgroundHandler Backgrounds => m_backgroundHandler;


		protected override void SingletonAwake()
		{
			PerScene();
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
			var animator = BackgroundHandler.GetComponentCustom<Animator>(transitionObject);
			var transitionSlave = BackgroundHandler.GetComponentCustom<TransitionSlave>(transitionObject);
			return (animator, transitionSlave);
		}

		public async Task FadeIn(int index, float fadeMultiplier = 1f)
		{
			var (animator, transitionSlave) = GetAndVerifyAnimator(transitions[index]);
			animator.speed = fadeMultiplier;
			await transitionSlave.StartAnimation();
		}
		public async Task FadeOut(int index, float fadeMultiplier = 1f)
		{
			var (animator, transitionSlave) = GetAndVerifyAnimator(transitions[index]);
			animator.speed = fadeMultiplier * -1;
			await transitionSlave.StopAnimation();
		}

		private Action<TransitionStatus> finalizeValue = null;
		public void FinishedAnimation(TransitionStatus isBeginning)
		{
			finalizeValue?.Invoke(isBeginning);
		}
		private TaskCompletionSource<TransitionStatus> GetCompletionTransitionTask(TransitionStatus expectedValue)
		{
			var completionSource = new TaskCompletionSource<TransitionStatus>();
			finalizeValue = value =>
			{
				if (value != expectedValue)
					return;
				completionSource.SetResult(value);
				finalizeValue = null;
			};
			return completionSource;
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
				BGCanvas = (obj, GetComponentCustom<Image>(obj), GetComponentCustom<VideoPlayer>(obj));
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

			public static T GetComponentCustom<T>(GameObject parent) where T : UnityEngine.Object
			{
				T output = parent.GetComponent<T>();
				if (output == null)
				{
					output = parent.GetComponentsInChildren<T>().Single();
					if (output == null)
						throw new NullReferenceException();
				}
				return output;
			}
		}
	}
}