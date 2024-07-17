#if UNITY_EDITOR
namespace Live2D.Cubism.Framework.Editor
{
	using System;
	using UnityEditor;
	using UnityEngine;
	using Object = UnityEngine.Object;

	/// <summary>
	/// A class that automatically deals with Dirty calling & serialization for you.
	/// </summary>
	public static class DirtyAuto
	{
		/// <summary>
		/// Sets an object dirty or not.
		/// </summary>
		/// <param name="target"> The object to set to dirty. </param>
		/// <param name="clearDirty"> If it should clear the dirtyness or not uwu </param>
		public static void SetDirty(this Object target, bool clearDirty = false)
		{
			if (clearDirty)
				EditorUtility.SetDirty(target);
			else
				EditorUtility.ClearDirty(target);
		}
		public static bool Toggle(in Object target, in GUIContent content, in bool input)
		{
			bool output = EditorGUILayout.Toggle(content, input);
			if (output != input)
				EditorUtility.SetDirty(target);
			return output;
		}
		public static bool ToggleLeft(in Object target, in GUIContent content, in bool input)
		{
			bool output = EditorGUILayout.ToggleLeft(content, input);
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
			// Enum.Parse<TComponent> doesn't like me
			return (TEnum)Enum.Parse(typeof(TEnum), enumNames[newIndex]);
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
		public static T Field<T>(in Object target, in GUIContent content, in T input, bool assignSceneObjects) where T : Object
		{
			T output = (T)EditorGUILayout.ObjectField(content, input, typeof(T), assignSceneObjects);
			if (output != input)
				EditorUtility.SetDirty(target);
			return output;
		}
		public static Color Field(in Object target, in GUIContent content, in Color input)
		{
			Color output = EditorGUILayout.ColorField(content, input);
			if (output != input)
				EditorUtility.SetDirty(target);
			return output;
		}
		public static Vector3 Field(in Object target, in GUIContent content, in Vector3 input)
		{
			Vector3 output = EditorGUILayout.Vector3Field(content, input);
			if (output != input)
				EditorUtility.SetDirty(target);
			return output;
		}
		public static string Field(in Object target, in GUIContent content, in string input)
		{
			string output = EditorGUILayout.TextField(content, input);
			if (output != input)
				EditorUtility.SetDirty(target);
			return output;
		}
		public static string Field(Rect rect, in Object target, in GUIContent content, in string input)
		{
			string output = EditorGUI.TextField(rect, content, input);
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
		public static int Field(Rect rect, in Object target, in GUIContent content, in int input)
		{
			int output = EditorGUI.IntField(rect, content, input);
			if (output != input)
				EditorUtility.SetDirty(target);
			return output;
		}
		public static float Field(Rect rect, in Object target, in GUIContent content, in float input)
		{
			float output = EditorGUI.FloatField(rect, content, input);
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
		public static void Property(in SerializedObject serializedObject, string fieldName)
		{
			serializedObject.Update();
			EditorGUILayout.PropertyField(serializedObject.FindProperty(fieldName));
			serializedObject.ApplyModifiedProperties();
		}
		public static void Property(Rect rect, in SerializedObject serializedObject, string fieldName)
		{
			serializedObject.Update();
			EditorGUI.PropertyField(rect, serializedObject.FindProperty(fieldName));
			serializedObject.ApplyModifiedProperties();
		}
		public static string Area(Rect rect, in Object target, in string input)
		{
			string output = EditorGUI.TextArea(rect, input);
			if (output != input)
				EditorUtility.SetDirty(target);
			return output;
		}
		public static string Area(Rect rect, in Object target, in GUIContent content, in string input)
		{
			Rect labelRect = rect;
			labelRect.height = 20f;
			rect.yMin += 20f;
			EditorGUI.LabelField(rect, content);
			string output = EditorGUI.TextArea(rect, input);
			if (output != input)
				EditorUtility.SetDirty(target);
			return output;
		}
		public static string Area(in Object target, GUIContent content, in string input)
		{
			EditorGUILayout.LabelField(content);
			return Area(target, content, input);
		}
		public static string Area(in Object target, in string input)
		{
			string output = EditorGUILayout.TextArea(input);
			if (output != input)
				EditorUtility.SetDirty(target);
			return output;
		}
	}
}
#endif