using UnityEditor;

namespace Kamgam.ExcludeFromBuild
{
    public class ConditionCallback
    {
        private System.Action _callback;
        private System.Func<bool> _condition;

        public ConditionCallback(System.Action function, System.Func<bool> condition)
        {
            _callback = function;
            _condition = condition;

            if (_callback != null && _condition != null)
                EditorApplication.update += update;
        }

        public static ConditionCallback Create(System.Action function, System.Func<bool> condition)
        {
            return new ConditionCallback(function, condition);
        }

        public void Clear()
        {
            EditorApplication.update -= update;
        }

        private void update()
        {
            if (_condition != null && _condition.Invoke())
            {
                var func = _callback;
                Clear();
                func?.Invoke();
            }
        }
    }
}
