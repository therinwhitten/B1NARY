namespace B1NARY.UI
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using TMPro;
	using UnityEngine;

	public class InputAutoActivator : MonoBehaviour
	{
		public TMP_InputField field;
		
		private void Reset()
		{
			field = GetComponent<TMP_InputField>();
		}

		private void OnEnable()
		{
			field.ActivateInputField();
		}
	}
}
