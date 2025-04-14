using UnityEngine;
using UnityEngine.SceneManagement;
using Character_Controllers; // Ensure this matches the namespace of your ForceModelName script

namespace CrossScene_SceneManager
{
    [DisallowMultipleComponent]
    public class ForceModelNameCrossoverScene : MonoBehaviour
    {
        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            FixAllNames();
        }

        private void FixAllNames()
        {
            // Find all ForceModelName components, including inactive objects
            ForceModelName[] forceNameComponents = GameObject.FindObjectsOfType<ForceModelName>(true);

            foreach (var forceNameComponent in forceNameComponents)
            {
                // Temporarily activate the GameObject to apply name fixes
                if (!forceNameComponent.gameObject.activeSelf)
                {
                    forceNameComponent.gameObject.SetActive(true);
                    forceNameComponent.InvokeFixNameOnInactive();
                    forceNameComponent.gameObject.SetActive(false);
                }
                else
                {
                    forceNameComponent.InvokeFixNameOnInactive();
                }
            }
        }
    }
}