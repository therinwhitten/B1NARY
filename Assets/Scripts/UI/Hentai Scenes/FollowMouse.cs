﻿namespace B1NARY
{
    using System.Collections;
	using UnityEngine;
	using UI;

	public  class FollowMouse : MonoBehaviour 
	{
		// Mouse Tracking Grabby Hand!
		Vector3 pos;
		public float speed = 1f;

		public void Update()
        {
            pos = Input.mousePosition;
			pos.z = speed;
			transform.position = Camera.main.ScreenToWorldPoint(pos);
        }
	}
}