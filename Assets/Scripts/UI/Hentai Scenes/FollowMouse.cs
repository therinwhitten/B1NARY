namespace B1NARY
{
    using UnityEngine.InputSystem;
	using UnityEngine;
	using UI;

	public class FollowMouse : MonoBehaviour 
	{
		// Mouse Tracking Grabby Hand!  (Not Finished Yet: Erase when tested and verified)
		Vector3 pos;
		public float speed = 1f;

		public void Update()
        {
           Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
           mousePosition.z = Camera.main.transform.position.z + Camera.main.nearClipPlane;
           transform.position = mousePosition;
	    }
	}	
}