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

		private InputSystemUIInputModule module;
		public Camera[] targetedCameras;
		private float[] originalSizes;

		private Vector3 originalPoint;
		private Rect OriginRange 
		{ 
			get
			{
				float range = originalSizes[0] - targetedCameras[0].orthographicSize;
				Vector2 range2 = new Vector2(range, range);
				Rect output = new Rect(originalPoint, range2);
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
			module = FindObjectOfType<InputSystemUIInputModule>();
			module.scrollWheel.action.performed += OnScrollWheel;
			module.leftClick.action.performed += OnStartHold;
		}
		private void OnDisable()
		{
			module.scrollWheel.action.performed -= OnScrollWheel;
			module.leftClick.action.performed -= OnStartHold;
		}
		private void OnScrollWheel(InputAction.CallbackContext callbackContext)
		{
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
				Vector2 original = targetedCameras[0].ScreenToWorldPoint(module.point.action.ReadValue<Vector2>());
				while (callbackContext.action.IsPressed())
				{
					Vector2 @new = targetedCameras[0].ScreenToWorldPoint(module.point.action.ReadValue<Vector2>());
					Vector2 delta = original - @new;
					delta *= speedMultiplier;
					delta /= currentMagnification;
					original = @new + delta;
					SetOriginPosition((Vector3)delta + transform.position);
					Debug.Log("sex");
					yield return new WaitForEndOfFrame();
				}
			}
		}
		
		public void SetOriginPosition(Vector2 newOriginPosition)
		{
			Rect range = OriginRange;
			Vector3 newPos = new Vector3(newOriginPosition.x, newOriginPosition.y, transform.position.z);
			newPos.x = Mathf.Clamp(newPos.x, range.x - range.width, range.x + range.width);
			newPos.y = Mathf.Clamp(newPos.y, range.y - range.height, range.y + range.height);
			transform.position = newPos;
		}
	}
}