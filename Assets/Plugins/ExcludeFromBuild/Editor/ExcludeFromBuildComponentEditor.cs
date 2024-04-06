using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Kamgam.ExcludeFromBuild
{
    [CustomEditor(typeof(ExcludeFromBuildComponent))]
    public class ExcludeFromBuildComponentEditor : Editor
    {
        ExcludeFromBuildComponent comp; 

        SerializedProperty objectProp; // GameObject
        SerializedProperty componentsProp; // Components
        SerializedProperty allGroupsProp; // AllGroups
        SerializedProperty simluateProp; // SimulateInPlayMode
        SerializedProperty groupIdsProp;

        DrawUtils.MultiSelectFromListGUI groupsMultiSelect = new DrawUtils.MultiSelectFromListGUI();

        public void Awake()
        {
            // TODO: Investigate: this also shows a huge icon in the scene view, which is not what we want. Is there a way to avoid that?
            var settings = ExcludeFromBuildSettings.GetOrCreateSettings();
            if (settings.ShowIconOnComponent)
                DrawUtils.SetIcon(target, DrawUtils.GetExcludeIconTexture());
            else
                DrawUtils.SetIcon(target, null);
        }

        public void OnEnable()
        {
            comp = target as ExcludeFromBuildComponent;

            objectProp = serializedObject.FindProperty("GameObject");
            componentsProp = serializedObject.FindProperty("Components");
            allGroupsProp = serializedObject.FindProperty("AllGroups");
            simluateProp = serializedObject.FindProperty("TestInPlayMode");
            groupIdsProp = serializedObject.FindProperty("GroupIds");
        }

        public override void OnInspectorGUI()
        {
            var data = ExcludeFromBuildData.GetOrCreateData();
            
            serializedObject.Update();
            bool hasChanged = false;

            GUILayout.Label("Excluded Objects, Components");
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.PropertyField(objectProp);
            if (!objectProp.boolValue)
            {
                BeginHorizontalIndent(10);
                EditorGUILayout.PropertyField(componentsProp);
                EndHorizontalIndent();
            }
            EditorGUILayout.EndVertical();

            GUILayout.Label("Groups");
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.PropertyField(allGroupsProp);
            if (!allGroupsProp.boolValue)
            {
                EditorGUILayout.BeginHorizontal();
                groupsMultiSelect.DrawGroupsMultiSelectFromInts(comp.GroupIds, data.Groups, "Groups:", "The exclusions will only be executed if the currently active group is in the list.");
                if (groupsMultiSelect.HasChanged)
                {
                    // Actually not needed as the serializedObject would be updated
                    // within the next draw via serializedObject.Update(). Yet we
                    // do it here manually to ensure the serializedObject is up-to-date
                    // at the end of this frame.
                    // Sadly serializedObject.hasModifiedProperties does not pick up on
                    // the changes made. There for we need some custom changed flags (see below).
                    updatePropertyArrayValuesInt(groupIdsProp, comp.GroupIds);
                    // Reset groupsMultiSelect to detect future changes
                    groupsMultiSelect.HasChanged = false;
                    // Custom flag to recognize changed groupIds.
                    hasChanged = true;
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();

            GUILayout.Label("Testing");
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.PropertyField(simluateProp);
            if (simluateProp.boolValue)
            {
                if (!comp.GroupIds.Contains(data.CurrentGroup.Id))
                {
                    if (!allGroupsProp.boolValue)
                    {
                        var s = new GUIStyle(GUI.skin.label);
                        s.normal.textColor = Color.yellow;
                        s.wordWrap = true;
                        GUILayout.Label("Object and Components will not be excluded based on current active group (active group is '" + data.CurrentGroup.Name + "').", s);
                    }
                }
            }
            EditorGUILayout.EndVertical();

            if (serializedObject.hasModifiedProperties || hasChanged)
            {
                EditorUtility.SetDirty(target);
                serializedObject.ApplyModifiedProperties();
            }
        }

        /// <summary>
        /// Returns true if the values have changed. False otherwise.
        /// </summary>
        /// <param name="prop"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        protected bool updatePropertyArrayValuesInt(SerializedProperty prop, IList<int> list)
        {
            if (!prop.isArray)
                throw new System.Exception("The property " + prop.name + " has to be an array.");

            bool changed = prop.arraySize != list.Count;

            prop.arraySize = list.Count;
            for (int i = 0; i < prop.arraySize; i++)
            {
                var subProp = prop.GetArrayElementAtIndex(i);
                if (subProp.intValue != list[i])
                    changed = true;
                
                subProp.intValue = list[i];
            }

            return changed;
        }

        static List<int> platformsMultiSelectIndices = new List<int>(10);

        public static int[] DrawPlatformsMultiSelect(int[] platformIds, string text, string tooltip, List<ExcludeFromBuildData.Group> platforms)
        {
            return DrawPlatformsMultiSelect(platformIds, new GUIContent(text, tooltip), platforms);
        }

        public static int[] DrawPlatformsMultiSelect(int[] platformIds, GUIContent content, List<ExcludeFromBuildData.Group> platforms)
        {
            // make options for
            string[] options = new string[platforms.Count+1];
            if (platforms != null && platforms.Count >= 0) 
            {
                for (int i = 0; i < platforms.Count; i++)
                {
                    options[i] = platforms[i].Name;
                }
            }

            int flags = 0;
            flags = EditorGUILayout.MaskField(content, flags, options);
            platformsMultiSelectIndices.Clear();
            for (int i = 0; i < options.Length; i++)
            {
                if ((flags & (1 << i)) == (1 << i))
                {
                    platformsMultiSelectIndices.Add(i);
                }
            }
            if (GUILayout.Button("Print Options"))
            {
                foreach (var o in platformsMultiSelectIndices)
                {
                    Debug.Log(o);
                }
            }

            return platformIds;
        }

        public static void BeginHorizontalIndent(int indentAmount = 10, bool beginVerticalInside = true)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(indentAmount);
            if (beginVerticalInside)
                GUILayout.BeginVertical();
        }

        public static void EndHorizontalIndent(float indentAmount = 10, bool begunVerticalInside = true, bool bothSides = false)
        {
            if (begunVerticalInside)
                GUILayout.EndVertical();
            if (bothSides)
                GUILayout.Space(indentAmount);
            GUILayout.EndHorizontal();
        }
    }
}
