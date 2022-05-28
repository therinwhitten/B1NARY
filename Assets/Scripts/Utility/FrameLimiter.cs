using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrameLimiter : MonoBehaviour
{
	// DON'T PUT THIS IN THE FINAL BUILD!!!!!!!!!!!!
	public int target = 120;

	void Awake()
	{
		QualitySettings.vSyncCount = 0;
		Application.targetFrameRate = target;
	}

	void Update()
	{
		if (Application.targetFrameRate != target)
			Application.targetFrameRate = target;
	}
}
