/**
 * Copyright(c) Live2D Inc. All rights reserved.
 *
 * Use of this source code is governed by the Live2D Open Software license
 * that can be found at https://www.live2d.com/eula/live2d-open-software-license-agreement_en.html.
 */


using UnityEngine;

namespace Live2D.Cubism.Framework.Expression
{
    public class CubismExpressionList : ScriptableObject
    {
        /// <summary>
        /// Cubism expression objects.
        /// </summary>
        [SerializeField]
        public CubismExpressionData[] CubismExpressionObjects;
#warning Added Values Here!
        public string[] ToNames()
        {
            Debug.Log(CubismExpressionObjects.Length);
			var expressions = new string[CubismExpressionObjects.Length];
			for (int i = 0; i < expressions.Length; i++)
			{
				string expression = CubismExpressionObjects[i].name;
				expressions[i] = expression.Remove(expression.LastIndexOf('.'));
			}
			return expressions;
		}
    }
}
