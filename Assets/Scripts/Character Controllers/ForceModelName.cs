using UnityEngine;
using Live2D.Cubism.Core;

namespace Character_Controllers
{
    [DisallowMultipleComponent]
    public class ForceModelName : MonoBehaviour
    {
        [SerializeField]
        private string expectedName;

        private string mocAutoName;
        private bool nameFixed;

        private void Awake()
        {
            if (string.IsNullOrEmpty(expectedName))
                expectedName = gameObject.name;

            mocAutoName = GetComponent<CubismModel>()?.name;
            FixName();
        }

        private void OnEnable()
        {
            FixName();
        }

        private void LateUpdate()
        {
            FixName();
        }

        // Ensure name fix can run when inactive (called manually from editor or other scripts)
        public void FixName()
        {
            if (!nameFixed && gameObject.name == mocAutoName && gameObject.name != expectedName)
            {
                gameObject.name = expectedName;
                nameFixed = true;
            }
        }

        // Manually call FixName when inactive
        public void InvokeFixNameOnInactive()
        {
            if (!nameFixed && gameObject.name == mocAutoName && gameObject.name != expectedName)
            {
                gameObject.name = expectedName;
                nameFixed = true;
            }
        }

#if UNITY_EDITOR
        public void SetExpectedNameInEditor()
        {
            expectedName = gameObject.name;
        }
#endif
    }
}