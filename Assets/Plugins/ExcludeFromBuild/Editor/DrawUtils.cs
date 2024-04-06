using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Kamgam.ExcludeFromBuild
{
    public static class DrawUtils
    {
        public static Texture2D GetExcludeIconTexture()
        {
            // Icon locations thanks to:
            // https://github.com/halak/unity-editor-icons

#if UNITY_2020_1_OR_NEWER
            var texture = EditorGUIUtility.FindTexture("CollabError");
#else
            var texture = EditorGUIUtility.FindTexture("d_P4_DeletedLocal");
#endif
            return texture;
        }

        public static void SetIcon(Object obj, Texture2D texture)
        {
            try
            {
#if UNITY_2022_1_OR_NEWER
                EditorGUIUtility.SetIconForObject(obj, texture);
#else
                var type = typeof(EditorGUIUtility);
                if (type == null)
                    return;

                var methodInfo = type.GetMethod("SetIconForObject", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                if (methodInfo == null)
                    return;

                methodInfo.Invoke(null, new object[] { obj, texture });
#endif
            }
            catch (System.Exception) { }
        }

        static Texture2D _texture0;

        public static Texture2D GetColorTexture0(Color col)
        {
            if (_texture0 == null)
            {
                Color[] pixels = new Color[2 * 2];
                for (int i = 0; i < pixels.Length; ++i)
                {
                    pixels[i] = col;
                }
                _texture0 = new Texture2D(2, 2);
                _texture0.SetPixels(pixels);
                _texture0.Apply();
            }

            return _texture0;
        }

        #region build target dropdowns
        static List<BuildTarget> _buildTargetValues;
        public static List<BuildTarget> BuildTargetValues
        {
            get
            {
                if (_buildTargetValues == null)
                {
                    _buildTargetValues = new List<BuildTarget>();
                    foreach (BuildTarget value in System.Enum.GetValues(typeof(BuildTarget)))
                        _buildTargetValues.Add(value);
                }

                return _buildTargetValues;
            }
        }

        static List<string> _buildTargetNames;
        public static List<string> BuildTargetNames
        {
            get
            {
                if (_buildTargetNames == null)
                {
                    _buildTargetNames = new List<string>();
                    var type = typeof(BuildTarget);
                    foreach (int value in System.Enum.GetValues(type))
                        _buildTargetNames.Add(System.Enum.GetName(type, value));
                }
                return _buildTargetNames;
            }
        }

        public static List<BuildTarget> DrawBuildTargetsMultiSelect(List<BuildTarget> targets, string text, string tooltip, Color? labelTextColor = null, bool drawLabel = true, params GUILayoutOption[] options)
        {
            return DrawBuildTargetsMultiSelect(targets, new GUIContent(text, tooltip), labelTextColor, drawLabel, options);
        }

        static List<BuildTarget> tmpBuildTargetsSelection;

        static void onBuildTargetsMultiSelectSelected(object userData)
        {
            var index = (int)userData;
            if (index == -2)
            {
                // Nothing
                tmpBuildTargetsSelection.Clear();
            }
            else if (index == -1)
            {
                // Everything
                tmpBuildTargetsSelection.Clear();
                for (int i = 0; i < BuildTargetValues.Count; i++)
                {
                    tmpBuildTargetsSelection.Add(BuildTargetValues[i]);
                }
            }
            else
            {
                var value = BuildTargetValues[index];

                if (tmpBuildTargetsSelection.Contains(value))
                {
                    tmpBuildTargetsSelection.Remove(value);
                }
                else
                {
                    tmpBuildTargetsSelection.Add(value);
                }
            }
        }

        public static List<BuildTarget> DrawBuildTargetsMultiSelect(List<BuildTarget> selection, GUIContent content, Color? labelTextColor = null, bool drawLabel = true, params GUILayoutOption[] options)
        {
            string buttonLabel = "Nothing ▾";

            if (selection != null && selection.Count > 0)
            {
                if (selection.Count == 1)
                    buttonLabel = selection[0].ToString() + " ▾";
                else if (selection.Count == BuildTargetValues.Count)
                    buttonLabel = "Everything ▾";
                else
                    buttonLabel = "Mixed ▾";
            }

            if (drawLabel)
            {
                var style = new GUIStyle(GUI.skin.label);
                if (labelTextColor.HasValue)
                    style.normal.textColor = labelTextColor.Value;
                GUILayout.Label(content, style);
            }

            if (GUILayout.Button(new GUIContent(buttonLabel, content.tooltip), options))
            {
                tmpBuildTargetsSelection = selection;

                var selectedMenu = new GenericMenu();
                selectedMenu.AddItem(new GUIContent("Nothing"), selection == null || selection.Count == 0, onBuildTargetsMultiSelectSelected, -2);
                selectedMenu.AddItem(new GUIContent("Everything"), selection != null && selection.Count == BuildTargetValues.Count, onBuildTargetsMultiSelectSelected, -1);
                selectedMenu.AddSeparator("");
                for (var i = 0; i < BuildTargetValues.Count; ++i)
                {
                    var menuString = BuildTargetNames[i];
                    var selected = selection.Contains(BuildTargetValues[i]);

                    selectedMenu.AddItem(new GUIContent(menuString), selected, onBuildTargetsMultiSelectSelected, i);
                }

                selectedMenu.ShowAsContext();
            }

            return selection;
        }

        public static ExcludeFromBuildData.StandaloneBuildSubtarget DrawStandaloneSubTargetMultiSelect(ExcludeFromBuildData.StandaloneBuildSubtarget subTarget, GUIContent content, Color? labelTextColor = null, bool drawLabel = true, params GUILayoutOption[] options)
        {
            if (drawLabel)
            {
                DrawLabel(content, labelTextColor);
            }

            // Dropdown
            return (ExcludeFromBuildData.StandaloneBuildSubtarget)EditorGUILayout.EnumPopup(subTarget, options);
        }

        public static void DrawLabel(GUIContent content, Color? labelTextColor, params GUILayoutOption[] options)
        {
            var style = new GUIStyle(GUI.skin.label);
            if (labelTextColor.HasValue)
                style.normal.textColor = labelTextColor.Value;
            GUILayout.Label(content, style, options);
        }
        #endregion

        #region Group multi select

        public class MultiSelectFromListGUI
        {
            List<int> tmpGroupsSelection;
            List<ExcludeFromBuildData.Group> tmpAllGroups;

            /// <summary>
            /// Will be set to true whenever the selectin changed.
            /// YOU are resposible for resetting it to false for continuous checking.
            /// </summary>
            public bool HasChanged = false;

            void onGroupSelected(object userData)
            {
                HasChanged = true;

                var index = (int)userData;
                if (index == -2)
                {
                    // Nothing
                    tmpGroupsSelection.Clear();
                }
                else if (index == -1)
                {
                    // Everything
                    tmpGroupsSelection.Clear();
                    for (int i = 0; i < tmpAllGroups.Count; i++)
                    {
                        tmpGroupsSelection.Add(tmpAllGroups[i].Id);
                    }
                }
                else
                {
                    var value = tmpAllGroups[index];
                    if (tmpGroupsSelection.Contains(value.Id))
                    {
                        tmpGroupsSelection.Remove(value.Id);
                    }
                    else
                    {
                        tmpGroupsSelection.Add(value.Id);
                    }
                }
            }


            public void DrawGroupsMultiSelectFromInts(List<int> groupIds, List<ExcludeFromBuildData.Group> allGroups, string text, string tooltip)
            {
                DrawGroupsMultiSelectFromInts(groupIds, allGroups, new GUIContent(text, tooltip));
            }

            public void DrawGroupsMultiSelectFromInts(List<int> groupIds, List<ExcludeFromBuildData.Group> allGroups, GUIContent content)
            {
                tmpGroupsSelection = groupIds;
                tmpAllGroups = allGroups;

                string buttonLabel = "None ▾";

                if (groupIds != null && groupIds.Count > 0)
                {
                    if (groupIds.Count == 1)
                    {
                        foreach (var group in tmpAllGroups)
                            if (group.Id == groupIds[0])
                                buttonLabel = group.Name + " ▾";
                    }
                    else if (groupIds.Count == tmpAllGroups.Count)
                        buttonLabel = "All Groups ▾";
                    else
                        buttonLabel = "Mixed ▾";
                }

                var data = ExcludeFromBuildData.GetOrCreateData();

                GUILayout.Label(content);
                if (GUILayout.Button(new GUIContent(buttonLabel, content.tooltip)))
                {
                    var selectedMenu = new GenericMenu();
                    selectedMenu.AddItem(new GUIContent("None"), groupIds == null || groupIds.Count == 0, onGroupSelected, -2);
                    selectedMenu.AddItem(new GUIContent("All Groups"), groupIds != null && groupIds.Count == tmpAllGroups.Count, onGroupSelected, -1);
                    selectedMenu.AddSeparator("");
                    for (var i = 0; i < tmpAllGroups.Count; ++i)
                    {
                        var menuString = tmpAllGroups[i].Name;
                        if (tmpAllGroups[i].Id == data.CurrentGroup.Id)
                            menuString += " *";

                        var selected = groupIds.Contains(tmpAllGroups[i].Id);

                        selectedMenu.AddItem(new GUIContent(menuString), selected, onGroupSelected, i);
                    }

                    selectedMenu.ShowAsContext();
                }
            }
        }
        #endregion
    }
}