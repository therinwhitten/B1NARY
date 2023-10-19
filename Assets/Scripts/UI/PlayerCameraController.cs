namespace B1NARY
{
	using System;
	using System.Collections;
	using UnityEngine;
	using UnityEngine.InputSystem;
	using UnityEngine.InputSystem.UI;
	using UnityEngine.Video;

	public class PlayerCameraController : MonoBehaviour
	{
		[Header("Scrolling")]
		public float speedMultiplier = 3.4f;

		/// <summary>
		/// The maximum zoom magnification that the camera can have.
		/// </summary>
		[Header("Zooming")]
		public float maxZoomMagnification = 16f;
		private float currentMagnification = 1f;

		public InputActionProperty Scroll;
		public InputActionProperty RightClick;
		public InputActionProperty MousePointer;
		public Camera[] targetedCameras;
		private float[] originalSizes;

		private Vector3 originalPoint;
		private Rect OriginRange 
		{ 
			get
			{
				float range = originalSizes[0] - targetedCameras[0].orthographicSize;
				Vector2 range2 = new(range, range);
				Rect output = new(originalPoint, range2);
				return output;
			} 
		}

		private void Start()
		{
			originalSizes = new float[targetedCameras.Length];
			for (int i = 0; i < targetedCameras.Length; i++)
				originalSizes[i] = targetedCameras[i].orthographicSize;
			originalPoint = targetedCameras[0].transform.position;
		}
		private void OnEnable()
		{
			Scroll.action.performed += OnScrollWheel;
			RightClick.action.performed += OnStartHold;
		}
		private void OnDisable()
		{
			Scroll.action.performed -= OnScrollWheel;
			RightClick.action.performed -= OnStartHold;
		}
		private void OnScrollWheel(InputAction.CallbackContext callbackContext)
		{
			if (!RightClick.action.IsPressed())
				return;
			float deltaScroll = callbackContext.ReadValue<Vector2>().y > 0f ? 1f : -1f;
			currentMagnification = Mathf.Clamp(currentMagnification + deltaScroll, 1f, maxZoomMagnification);
			for (int i = 0; i < targetedCameras.Length; i++)
				targetedCameras[i].orthographicSize = originalSizes[i] / currentMagnification;
			SetOriginPosition(transform.position);
		}

		private void OnStartHold(InputAction.CallbackContext callbackContext)
		{
			StartCoroutine(Enum());
			IEnumerator Enum()
			{
				Vector2 original = MousePointer.action.ReadValue<Vector2>();
				while (callbackContext.action.IsPressed())
				{
					Vector2 @new = MousePointer.action.ReadValue<Vector2>();
					Vector2 delta = original - @new;
					delta *= speedMultiplier;
					delta /= currentMagnification;
					original = @new;
					SetOriginPosition((Vector3)delta + transform.position);
					yield return new WaitForEndOfFrame();
				}
			}
		}
		
		public void SetOriginPosition(Vector2 newOriginPosition)
		{
			Rect range = OriginRange;
			Vector3 newPos = new(newOriginPosition.x, newOriginPosition.y, transform.position.z);
			newPos.x = Mathf.Clamp(newPos.x, range.x - range.width, range.x + range.width);
			newPos.y = Mathf.Clamp(newPos.y, range.y - range.height, range.y + range.height);
			transform.position = newPos;
		}
	}
}