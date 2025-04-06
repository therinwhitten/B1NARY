namespace Character_Controllers
{
    using UnityEngine;
    using Live2D.Cubism.Core;  // ← Needed for CubismModel reference

    /// <summary>
    /// Restores prefab name only if Unity auto-resets it to the moc model name.
    /// Lets your game logic rename it if needed.
    /// </summary>
    [DisallowMultipleComponent]
    public class ForceModelName : MonoBehaviour
    {
        [SerializeField]
        private string expectedName;

        private string mocAutoName;

        private void Awake()
        {
            if (string.IsNullOrEmpty(expectedName))
            {
                expectedName = gameObject.name;
            }

            // Get the name Unity seems to reset it to (typically the moc filename)
            mocAutoName = GetComponent<CubismModel>()?.name;
        }

        private void LateUpdate()
        {
            if (gameObject.name == mocAutoName && gameObject.name != expectedName)
            {
                gameObject.name = expectedName;
            }
        }
    }
}