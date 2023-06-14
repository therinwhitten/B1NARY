namespace B1NARY
{
	using System.Collections;
	using UnityEngine;
	using UI;
	using UnityEngine.InputSystem.UI;

	public class FollowMouse : MonoBehaviour 
	{
		// Mouse Tracking Grabby Hand!  (Not Finished Yet: Erase when tested and verified)
		Vector3 pos;
		public float speed = 1f;
		private InputSystemUIInputModule module;
		private void Awake()
		{
			module = FindObjectOfType<InputSystemUIInputModule>();
		}
		public void Update()
		{
			pos = module.point.action.ReadValue<Vector2>();
			pos.z = speed;
			transform.position = Camera.main.ScreenToWorldPoint(pos);
		}
	}
}