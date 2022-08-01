namespace B1NARY.UI
{
	using System;
	using System.Collections;
	using UnityEngine;
	using UnityEngine.InputSystem;

	[Obsolete("This class does not work, use stack overflow instead!"), RequireComponent(typeof(CanvasGroup))]
	public class DraggableWindow : MonoBehaviour
	{
		private new Camera camera;
		private RectTransform window;
		private CanvasGroup canvas;
		[SerializeField]
		private PlayerInput playerInput;
		private InputAction mousePosition;
		private InputAction mousePress;

		private void Awake()
		{
			canvas = GetComponent<CanvasGroup>();
			mousePosition = playerInput.actions.FindAction("Mouse Position", true);
			mousePress = playerInput.actions.FindAction("Mouse Press", true);
			mousePress.performed += StartDragging;
			camera = Camera.current;
			window = GetComponent<RectTransform>();
		}
		private void StartDragging(InputAction.CallbackContext callbackContext)
		{
			if (!canvas.blocksRaycasts)
				return;
			Vector2 oldMousePos = NewMousePosition();
			if (!RectTransformUtility.RectangleContainsScreenPoint(window, oldMousePos))
				return;
			bool isStillHeld = true;
			mousePress.canceled += StopHoldingButton;
			B1NARYConsole.Log(nameof(DraggableWindow), $"Started dragging window '{gameObject.name}'");
			StartCoroutine(DraggingCoroutine());
			IEnumerator DraggingCoroutine()
			{
				yield return new WaitForEndOfFrame();
				while (canvas.blocksRaycasts && isStillHeld)
				{
					Vector2 newMousePos = NewMousePosition();
					if (newMousePos == oldMousePos)
					{
						yield return new WaitForEndOfFrame();
						continue;
					}
					oldMousePos -= newMousePos;
					window.position -= (Vector3)oldMousePos;
					oldMousePos = newMousePos;
					yield return new WaitForEndOfFrame();
				}
				B1NARYConsole.Log(nameof(DraggableWindow), $"Stopped dragging window '{gameObject.name}'");
				mousePress.canceled -= StopHoldingButton;
			}
			void StopHoldingButton(InputAction.CallbackContext callbackContext1) => isStillHeld = false;
			Vector2 NewMousePosition()
			{
				RectTransformUtility.ScreenPointToLocalPointInRectangle(window,
					mousePosition.ReadValue<Vector2>(), camera, out var output);
				return output;
			}
		}
	}
}