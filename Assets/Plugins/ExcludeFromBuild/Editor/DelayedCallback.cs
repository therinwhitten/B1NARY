using UnityEditor;
using UnityEngine;

namespace Kamgam.ExcludeFromBuild
{
    public class DelayedCallback
    {
        private System.Action _callback;
        private double _callbackTime;
        private double _delay;
        private bool _logCoutdown;
        private int _countdownSecond = -1;

        public DelayedCallback(System.Action function, double delay, bool logCoutdown)
        {
            _callback = function;
            _callbackTime = EditorApplication.timeSinceStartup + delay;
            _logCoutdown = logCoutdown;
            _delay = delay;

            EditorApplication.update += update;
        }

        public static DelayedCallback Create(System.Action function, double delay, bool logCountdown)
        {
            return new DelayedCallback(function, delay, logCountdown);
        }

        public void Clear()
        {
            EditorApplication.update -= update;

            _callbackTime = 0.0;
            _callback = null;
            _logCoutdown = false;
            _delay = 0;
            _countdownSecond = -1;
        }

        private void update()
        {
            if (_logCoutdown)
            {
                int second = Mathf.CeilToInt((float)(_callbackTime - EditorApplication.timeSinceStartup));
                if (second != _countdownSecond)
                {
                    _countdownSecond = second;
                    float progress = 1f - (float)((_callbackTime - EditorApplication.timeSinceStartup) / _delay);
                    EditorUtility.DisplayProgressBar("Waiting to start build", "Exclude from Build is waiting for the timer to restart the build.", progress);
                    Debug.Log("ExcludeFromBuild: Countdown to build: " + second);
                }
            }

            if (EditorApplication.timeSinceStartup > _callbackTime)
            {
                var callback = _callback;
                Clear();

                EditorUtility.ClearProgressBar();

                callback?.Invoke();
            }
        }
    }
}
