using UnityEngine;
using UnityEngine.InputSystem.UI;
using System.Collections;

namespace B1NARY
{
    public class FollowMouse : MonoBehaviour
    {
        // Mouse Tracking Grabby Hand! (Not Finished Yet: Erase when tested and verified)
        Vector3 pos;
        public float speed = 1f;
        private InputSystemUIInputModule module;

        private void Start()
        {
            // Delay the initialization of the input system for a brief moment.
            StartCoroutine(InitializeInputSystem());
        }

        private IEnumerator InitializeInputSystem()
        {
            yield return new WaitForSeconds(0.1f); // Adjust the delay time as needed.

            // Now that we've waited for a moment, attempt to find the input system.
            module = FindObjectOfType<InputSystemUIInputModule>();

            // Check if the input system was found before using it.
            if (module != null)
            {
                // Input system is ready, start following the mouse.
                StartCoroutine(FollowMousePointer());
            }
            else
            {
                Debug.LogError("InputSystemUIInputModule not found. Make sure it's in your scene.");
            }
        }

        private IEnumerator FollowMousePointer()
        {
            while (true)
            {
                pos = module.point.action.ReadValue<Vector2>();
                pos.z = speed;
                transform.position = Camera.main.ScreenToWorldPoint(pos);
                yield return null; // This ensures that the following code runs once per frame.
            }
        }
    }
}
