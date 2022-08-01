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
	using UI;
	using DesignPatterns;

	[RequireComponent(typeof(Image), typeof(FadeController))]
	public class TransitionHandler : SingletonAlt<TransitionHandler>
	{
		protected override void SingletonAwake()
		{
			transitionImage = GetComponent<Image>();
			fadeController = GetComponent<FadeController>();
			_ = fadeController.FadeOut(0f); // just in case.
			transitionImage.material = transitionShader;
			DefineObjectsPerSceneChange(SceneManager.GetActiveScene().name);
		}
		private void Start()
		{
			try { transitionImage.material.GetFloat(fadePercentageName); }
			catch
			{
				B1NARYConsole.LogError(nameof(TransitionHandler), $"'{fadePercentageName}'" +
					$" does not exist! Please patch before a softlock occurs!");
			}
		}

		private void DefineObjectsPerSceneChange(string sceneName)
		{
			RedefineBGData();
			if (AutomaticallyAssignAnimatedBG)
				SetNewAnimatedBackground(sceneName);

			SwitchedScenes += DefineObjectsPerSceneChange;
		}

		// Transitioning stuff, with backgrounds n such
		private Image transitionImage;
		private FadeController fadeController;

		public Material transitionShader;
		private Ref<float> TransitionValue
		{
			get
			{
				transitionImage.material.GetFloat(fadePercentageName);
				return new Ref<float>(
					() => transitionImage.material.GetFloat(fadePercentageName),
					set => transitionImage.material.SetFloat(fadePercentageName, set));
			}
		}

		public string fadePercentageName = "_Cutoff";
		public bool useTransitionValue = true;
		public Sprite textureIn, textureOut;
		private bool hasFadedIn = false;

		public Task Transition(float fadeTime, Action loadActions)
		{
			FadeInTransition(fadeTime);
			loadActions?.Invoke();
			FadeOutTransition(fadeTime);
			return Task.CompletedTask;
		}

		public async void FadeInTransition(float fadeIn)
		{
			if (hasFadedIn)
				throw new ArgumentException($"{nameof(TransitionHandler)} has already faded in!");
			hasFadedIn = true;
			transitionImage.sprite = textureIn;
			if (useTransitionValue)
				await Task.WhenAll(fadeController.FadeIn(fadeIn), this.ChangeFloat(TransitionValue, 1, fadeIn));
			else
				await fadeController.FadeIn(fadeIn);
		}

		public async void FadeOutTransition(float fadeOut)
		{
			if (!hasFadedIn)
				throw new ArgumentException($"{nameof(TransitionHandler)} has already faded out!");
			hasFadedIn = false;
			transitionImage.sprite = textureOut;
			if (useTransitionValue)
				await Task.WhenAll(fadeController.FadeOut(fadeOut), this.ChangeFloat(TransitionValue, 0, fadeOut));
			else
				await fadeController.FadeOut(fadeOut);
		}

		// Backgrounds

		public string backgroundCanvasName = "BG-Canvas";
		private VideoPlayer animatedBG;
		public bool LoopingAnimBG { get => animatedBG.isLooping; set => animatedBG.isLooping = value; }
		private ResourcesAsset<VideoClip> animatedBGData;
		private Image staticBG;
		private ResourcesAsset<Sprite> staticBGData;
		public bool AutomaticallyAssignAnimatedBG = true;

		private void RedefineBGData()
		{
			GameObject backgroundCanvas = GameObject.Find(backgroundCanvasName);
			animatedBG = backgroundCanvas.GetComponent<VideoPlayer>();
			staticBG = backgroundCanvas.GetComponent<Image>();
		}
		public void SetNewAnimatedBackground(string animBackgroundPath)
		{
			animatedBGData = new ResourcesAsset<VideoClip>("Backgrounds/" + animBackgroundPath, false);
			if (animatedBGData.target != null)
			{
				animatedBG.Stop();
				animatedBG.clip = animatedBGData;
				animatedBG.Play();
			}
			else
				B1NARYConsole.LogWarning(nameof(TransitionHandler),
					$"'Backgrounds/{animBackgroundPath}' is not a valid animated Background Path!");
		}

		public void SetNewStaticBackground(string staticBackgroundPath)
		{
			staticBGData = new ResourcesAsset<Sprite>("Backgrounds/" + staticBackgroundPath, false);
			if (staticBGData.target == null)
				return;
			staticBG.sprite = staticBGData;
		}

		// Scene Management
		public static bool CommandsAllowed { get; private set; } = true;
		private static Scene CurrentScene => SceneManager.GetActiveScene();
		public async Task TransitionToNextScene(string newSceneName, float fadeTime)
		{
			B1NARYConsole.Log(nameof(TransitionHandler), "Transitioning scene from "
				+ $"'{CurrentScene.name}' to '{newSceneName}'");
			CommandsAllowed = false;
			Task prepSwitchScenes = Task.Run(PrepareSwitchScenes);
			FadeInTransition(fadeTime);
			await Task.WhenAny(prepSwitchScenes, Task.Delay(100));
			AsyncOperation asyncOperation = SwitchScenes(newSceneName);
			SpinWait.SpinUntil(() => asyncOperation.isDone);
			FadeOutTransition(fadeTime);
			CommandsAllowed = true;
		}

		private static void PrepareSwitchScenes()
		{
			if (PreppedSwitchScenes)
				return;
			B1NARYConsole.Log(nameof(TransitionHandler), "Prepping to switch scenes,"
				+ " Launching methods tied to it.");
			PreppedSwitchScenes = true;
			// Some events wants to constantly listen to scene switches, so they usually
			// - re-assign the same method. This allows them to have the ability to
			// - easily do so without having a 'buffer' or wait until it is finished
			// - emptying out the event.
			if (SwitchingScenes == null)
				return;
			Action[] events = SwitchingScenes.GetInvocationList().Cast<Action>().ToArray();
			SwitchingScenes = null;
			Array.ForEach(events, del => del.Invoke());
		}
		public static event Action SwitchingScenes;
		public static bool PreppedSwitchScenes { get; private set; } = false;

		private static AsyncOperation SwitchScenes(string sceneName)
		{
			if (!PreppedSwitchScenes)
				return null;
			PreppedSwitchScenes = false;
			AsyncOperation sceneTask = SceneManager.LoadSceneAsync(sceneName);
			if (SwitchedScenes != null)
			{
				Action<string>[] events = SwitchedScenes.GetInvocationList()
					.Cast<Action<string>>().ToArray();
				SwitchedScenes = null;
				Array.ForEach(events, del => del.Invoke(sceneName));
			}
			return sceneTask;
		}
		public static event Action<string> SwitchedScenes;
	}
}