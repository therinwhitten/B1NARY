using UnityEngine;
using DG.Tweening;

namespace B1NARY.UI
{
    public class FloatAnimation : MonoBehaviour
    {
        public float floatAmount = 20f; // Adjust this value to change the amount of float
        public float floatDuration = 1f; // Adjust this value to change the duration of float
        public float floatAngle = 0f; // Adjust this value to set the float direction in degrees
        public float startDelay = 0f; // Adjust this value to set the delay before starting the animation

        private Vector3 initialLocalPosition;

        void Start()
        {
            initialLocalPosition = transform.localPosition;

            // Call the StartFloatAnimation method with delay
            Invoke("StartFloatAnimation", startDelay);
        }

        void StartFloatAnimation()
        {
            // Convert floatAngle to radians
            float radians = Mathf.Deg2Rad * floatAngle;

            // Calculate the target local position relative to the initial local position
            Vector3 targetLocalPosition = new Vector3(
                Mathf.Cos(radians) * floatAmount + initialLocalPosition.x,
                Mathf.Sin(radians) * floatAmount + initialLocalPosition.y,
                initialLocalPosition.z
            );

            // Define the float animation sequence using Dotween
            transform.DOLocalMove(targetLocalPosition, floatDuration)
                .SetEase(Ease.InOutQuad) // Change the easing function if desired
                .SetLoops(-1, LoopType.Yoyo); // Loop the animation indefinitely
        }
    }
}
