namespace B1NARY.CharacterManagement
{
	using B1NARY.CharacterManagement;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using UnityEngine;

	public class CharacterEmotionInvoker : MonoBehaviour
	{
		public Live2DActor actor;
		public void ChangeExpression(string expression)
		{
			actor.CurrentExpression = expression;
		}
		public void ChangeExpression(int index)
		{
			actor.expressionController.CurrentExpressionIndex = index;
		}
	}
}
