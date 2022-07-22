using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class TransitionHandler : SingletonAlt<TransitionHandler>
{
	private Image transitionImage;

	[Header("Transitions")] public Material transitionShader;
	private Ref<float> TransitionValue => new Ref<float>(
		() => transitionImage.material.GetFloat(fadePercentageName),
		set => transitionImage.material.SetFloat(fadePercentageName, set));

	public string fadePercentageName = "_Cutoff";
	public Sprite textureIn, textureOut;
	private bool hasFadedIn = false;

	[Header("Backgrounds")] public 

	protected override void SingletonAwake()
	{
		transitionImage = GetComponent<Image>();
		transitionImage.material = transitionShader;
	}

	// Transitioning stuff

	public Task Transition(float fadeTime, Action loadActions)
	{
		FadeInTransition(fadeTime);
		loadActions?.Invoke();
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


}