#if UNITY_EDITOR
namespace B1NARY.Editor
{
	using System.Collections.Generic;
	using System.Linq;
	using UnityEditor;
	using UnityEngine;

	public static class ExtraEditorTools
	{
		private static int? ButtonArray(GUIContent[] data, int xCount, Rect? rect = null)
		{
			// 4 xCount, four elements on one line, 6 Buttons. 
			Rect masterRect = rect ?? GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth, 20);
			int lines = (data.Length / xCount) + (data.Length % xCount != 0 ? 1 : 0);
			//internalRect.width /= lines;
			bool[] buttonData = new bool[data.Length];
			for (int i = 0; i < data.Length; i++)
			{
				int currentLinePoint = (data.Length / xCount) + (data.Length % xCount != 0 ? 1 : 0);
				float width = masterRect.width / xCount,
					height = masterRect.height / lines,
					yPos = masterRect.y + ((int)width * (currentLinePoint - 1)),
					xPos = (i % (xCount + 1)) + masterRect.x;
				Rect buttonRect = new(xPos, yPos, width, height);
				buttonData[i] = GUI.Button(buttonRect, data[i]);
			}
			int iterativeDictionary = -1;
			return buttonData.ToDictionary(useless => { iterativeDictionary++; return iterativeDictionary; })
				.Where(pair => pair.Value == true).First().Key;
		}



		public static int? ButtonGrid(string[] names, int xCount)
			=> ButtonArray(names.ToGUIContent(), xCount);





		public static GUIContent ToGUIContent(this string input) => new(input);
		public static GUIContent[] ToGUIContent(this IEnumerable<string> input)
			=> input.Select(line => new GUIContent(line)).ToArray();
	}
}
#endif