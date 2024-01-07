namespace B1NARY
{
    using System;
    using UnityEngine;
    using UnityEngine.UI;
    using UnityEngine.Video;
    using UnityEngine.SceneManagement;
    using B1NARY.DesignPatterns;
    using System.Collections;
    using B1NARY.UI;
    using System.Collections.Generic;
    using B1NARY.Scripting;

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
                InstanceOrDefault.SetNewStaticBackground("Backgrounds/" + backgroundName);
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
        private Image _staticBackground;
        private VideoPlayer _animatedBackground;

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

        public bool SetNewStaticBackground(string resourcesPath)
        {
            Sprite sprite = Resources.Load<Sprite>(resourcesPath);
            if (sprite == null)
                return false;

            // Set the static background sprite
            StaticBackground = sprite;

            // Stop the VideoPlayer and set its clip to null
            _animatedBackground.Stop();
            _animatedBackground.clip = null;

            // Explicitly activate the Image GameObject
            _staticBackground.gameObject.SetActive(true);

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
            SceneManager.InstanceOrDefault.SwitchedScenes.AddPersistentListener(UpdateBackgroundReferences);
            UpdateBackgroundReferences(); // Initial update

            // Add an additional listener to handle subsequent scene changes
            SceneManager.InstanceOrDefault.SwitchedScenes.AddPersistentListener(() =>
            {
                UpdateBackgroundReferences();
            });
        }

        private void UpdateBackgroundReferences()
        {
            var obj = GameObject.Find(BGCanvasName);
            if (obj == null)
            {
                Debug.LogError($"BGCanvas {BGCanvasName} not found in the current scene.");
                return;
            }

            _staticBackground = obj.GetComponentInChildren<Image>();
            _animatedBackground = obj.GetComponentInChildren<VideoPlayer>();

            if (_staticBackground == null)
            {
                Debug.LogError($"Image component not found on BGCanvas {BGCanvasName}. Make sure it's attached.");
            }

            if (_animatedBackground == null)
            {
                Debug.LogError($"VideoPlayer component not found on BGCanvas {BGCanvasName}. Make sure it's attached.");
            }
        }

        private void LateUpdate()
        {
            QueueUpdate();
            CheckForSceneChange();
        }

        private void CheckForSceneChange()
        {
            // Additional logic for handling scene change, if needed
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
}
