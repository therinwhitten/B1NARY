namespace B1NARY.UI
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using UnityEngine;

	public class DisableDelayer : MonoBehaviour
	{
		public float delay = 1f;

		private float m_currentDelay = float.NaN;

		private void OnEnable()
		{
			m_currentDelay = delay;
		}
		private void Update()
		{
			m_currentDelay -= Time.deltaTime;
			if (m_currentDelay < 0.0000001f)
				gameObject.SetActive(false);
		}
	}
}
