using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(Image))]
public class TransitionHandler : SingletonAlt<TransitionHandler>
{
	protected override void SingletonAwake()
	{
		transitionImage = GetComponent<Image>();
		transitionImage.material = transitionShader;
		DefineObjectsPerSceneChange(SceneManager.GetActiveScene().name);
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

	public Material transitionShader;
	private Ref<float> TransitionValue => new Ref<float>(
		() => transitionImage.material.GetFloat(fadePercentageName),
		set => transitionImage.material.SetFloat(fadePercentageName, set));
	public string fadePercentageName = "_Cutoff";
	public Sprite textureIn, textureOut;
	private bool hasFadedIn = false;

	public Task Transition(float fadeTime, Action loadActions)
	{
		FadeInTransition(fadeTime);
		loadActions.Invoke();
		FadeOutTransition(fadeTime);
		return Task.CompletedTask;
	}

	public void FadeInTransition(float fadeIn)
	{
		if (hasFadedIn)
			throw new ArgumentException($"{nameof(TransitionHandler)} has already faded in!");
		hasFadedIn = true;
		transitionImage.sprite = textureIn;
		this.ChangeFloat(TransitionValue, 1, fadeIn).Wait();
	}

	public void FadeOutTransition(float fadeOut)
	{
		if (!hasFadedIn)
			throw new ArgumentException($"{nameof(TransitionHandler)} has already faded out!");
		hasFadedIn = false;
		transitionImage.sprite = textureOut;
		this.ChangeFloat(TransitionValue, 0, fadeOut).Wait();
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
		animatedBG.Stop();
		animatedBG.clip = null;
		if (animatedBGData.target != null)
		{
			animatedBG.clip = animatedBGData;
			animatedBG.Play();
		}
		else
			$"'{animBackgroundPath}' is not a valid animated Background Path!".LogWarn();
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
		CommandsAllowed = false;
		Task prepSwitchScenes = Task.Run(PrepareSwitchScenes);
		FadeInTransition(fadeTime);
		await Task.WhenAny(prepSwitchScenes, Task.Delay(100));
		await SwitchScenes(newSceneName);
		DialogueSystem.Instance.initialize();
		CharacterManager.Instance.initialize();
		FadeOutTransition(fadeTime);
		CommandsAllowed = true;
	}

	private static void PrepareSwitchScenes()
	{
		if (PreppedSwitchScenes)
			return;
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

	private static Task SwitchScenes(string sceneName)
	{
		if (!PreppedSwitchScenes)
			return null;
		PreppedSwitchScenes = false;
		Task sceneTask = Task.Run(() => SceneManager.LoadScene(sceneName));
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