﻿namespace B1NARY.Editor
{
	using System;
	using UnityEditor;
	using UnityEngine;
	using Object = UnityEngine.Object;

	public static class DirtyAuto
	{
		public static Vector3 Field(in Object target, in GUIContent content, in Vector3 input)
		{
			Vector3 output = EditorGUILayout.Vector3Field(content, input);
			if (output != input)
				EditorUtility.SetDirty(target);
			return output;
		}
		public static bool Toggle(in Object target, in GUIContent content, in bool input)
		{
			bool output = EditorGUILayout.Toggle(content, input);
			if (output != input)
				EditorUtility.SetDirty(target);
			return output;
		}
		public static int Popup(in Object target, in GUIContent content, in int oldIndex, in string[] values)
		{
			int newIndex = EditorGUILayout.Popup(content, oldIndex, values);
			if (oldIndex != newIndex)
				EditorUtility.SetDirty(target);
			return newIndex;
		}
		public static TEnum Popup<TEnum>(in Object target, in GUIContent content, TEnum input) where TEnum : Enum
		{
			string[] enumNames = Enum.GetNames(typeof(TEnum));
			int index = Array.IndexOf(enumNames, input.ToString());
			int newIndex = EditorGUILayout.Popup(content, index, enumNames);
			if (index != newIndex)
				EditorUtility.SetDirty(target);
			return (TEnum)Enum.Parse(typeof(TEnum), enumNames[newIndex]);
		}
		public static string Field(in Object target, in GUIContent content, in string input)
		{
			string output = EditorGUILayout.TextField(content, input);
			if (output != input)
				EditorUtility.SetDirty(target);
			return output;
		}
		public static string Field(in Rect rect, in Object target, in GUIContent content, in string input)
		{
			string output = EditorGUI.TextField(rect, content, input);
			if (output != input)
				EditorUtility.SetDirty(target);
			return output;
		}
		public static float DelayedField(in Object target, in GUIContent content, in float input)
		{
			float output = EditorGUILayout.DelayedFloatField(content, input);
			if (output != input)
				EditorUtility.SetDirty(target);
			return output;
		}
		public static int DelayedField(in Object target, in GUIContent content, in int input)
		{
			int output = EditorGUILayout.DelayedIntField(content, input);
			if (output != input)
				EditorUtility.SetDirty(target);
			return output;
		}
		public static int Field(in Object target, in GUIContent content, in int input)
		{
			int output = EditorGUILayout.IntField(content, input);
			if (output != input)
				EditorUtility.SetDirty(target);
			return output;
		}
		public static float Field(in Object target, in GUIContent content, in float input)
		{
			float output = EditorGUILayout.FloatField(content, input);
			if (output != input)
				EditorUtility.SetDirty(target);
			return output;
		}
		public static float Slider(in Object target, in GUIContent content, in float input, in float left, in float right)
		{
			float output = EditorGUILayout.Slider(content, input, left, right);
			if (output != input)
				EditorUtility.SetDirty(target);
			return output;
		}
		public static int Slider(in Object target, in GUIContent content, in int input, in int left, in int right)
		{
			int output = EditorGUILayout.IntSlider(content, input, left, right);
			if (output != input)
				EditorUtility.SetDirty(target);
			return output;
		}
	}
}