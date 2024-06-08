using UnityEngine;
using UnityEditor;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using System.Linq;
using System;

namespace Kamgam.ExcludeFromBuild
{
    public static class BackgroundStyle
    {
        private static GUIStyle style = new GUIStyle();
        private static Texture2D texture;

        public static GUIStyle Get(Color color)
        {
            if (texture == null)
                texture = new Texture2D(1, 1);

            texture.SetPixel(0, 0, color);
            texture.Apply();
            style.normal.background = texture;

            return style;
        }
    }

    public class ExcludeFromBuildWindow : EditorWindow
    {
        public static GUIStyle ErrorGroupDetailsButtonBoxStyle;
        public static Color WarningTextColor = new Color(0.9f, 0.2f, 0.2f);

        private static Color _DefaultBackgroundColor;
        public static Color DefaultBackgroundColor
        {
            get
            {
                if (_DefaultBackgroundColor.a == 0)
                {
                    try
                    {
                        var method = typeof(EditorGUIUtility).GetMethod("GetDefaultBackgroundColor", BindingFlags.NonPublic | BindingFlags.Static);
                        _DefaultBackgroundColor = (Color)method.Invoke(null, null);
                    }
                    catch
                    {
                        // fallback if reflection fails
                        _DefaultBackgroundColor = new Color32(56, 56, 56, 255);
                    }
                }
                return _DefaultBackgroundColor;
            }
        }

        protected Vector2 contentScrollViewPos;
        protected bool isScanning;
        protected List<ExcludedObject> scanResults;
        protected List<string> scanLogs = new List<string>();
        protected ExcludeFromBuildController.ScenesToScan scanScenesForLastResult = ExcludeFromBuildController.ScenesToScan.OpenScenes;
        protected Vector2 scanLogScrollPos = Vector2.zero;
        protected bool scanLogsFoldout = false;
        protected ExcludeFromBuildController.ScenesToScan scanScenes = ExcludeFromBuildController.ScenesToScan.OpenScenes;
        protected System.Threading.CancellationTokenSource scanTokenSource;

        public static bool IsTesting
        {
            get
            {
                return SessionState.GetBool("Kamgam.ExcludeFromBuild.IsTesting", false);
            }

            set
            {
                SessionState.SetBool("Kamgam.ExcludeFromBuild.IsTesting", value);
            }
        }
        protected bool isEditingGroups;


        [MenuItem("Window/Exclude From Build")]
        static ExcludeFromBuildWindow openWindow()
        {
            return openWindow(focus: true);
        }

        static ExcludeFromBuildWindow openWindow(bool focus)
        {
            ExcludeFromBuildWindow window = (ExcludeFromBuildWindow)EditorWindow.GetWindow(typeof(ExcludeFromBuildWindow), focus);
            window.titleContent = new GUIContent("Exclude From Build");
            window.Initialize();
            window.Show();
            return window;
        }

        public static ExcludeFromBuildWindow GetOrOpen(bool focus = true)
        {
            if (!HasOpenInstances<ExcludeFromBuildWindow>())
            {
                var window = openWindow(focus);
                if (focus)
                    window.Focus();
                return window;
            }
            else
            {
                var window = GetWindow<ExcludeFromBuildWindow>("Exclude From Build", focus);
                if (focus)
                    window.Focus();
                return window;
            }
        }

        public void OnEnable()
        {
            Initialize();
        }

        public void Initialize()
        {
            ErrorGroupDetailsButtonBoxStyle = BackgroundStyle.Get(DefaultBackgroundColor);

            if (!isDocked())
            {
                if (position.width < 600 || position.height < 200)
                {
                    repositionWindow();
                }
            }

            isScanning = false;
            scanResults = null;
            scanLogs.Clear();
            scanScenesForLastResult = ExcludeFromBuildController.ScenesToScan.OpenScenes;
            scanScenes = ExcludeFromBuildController.ScenesToScan.OpenScenes;
            isEditingGroups = false;

            // check if an exclusion is hidden (if yes then assume a test is running)
            if (ExcludeFromBuildController.ExcludedObjectsAreHidden())
            {
                // Don't change test flag if it is a rebuild.
                if (!SessionState.GetBool("DontDelayBuildAgain", false))
                    IsTesting = true;
            }
        }

        private void repositionWindow()
        {
            const int width = 900;
            const int height = 700;
            var x = Screen.currentResolution.width / 2 - width;
            var y = Screen.currentResolution.height / 2 - height;
            position = new Rect(x, y, width, height);
        }

        protected bool isDocked()
        {
#if UNITY_2020_1_OR_NEWER
            return docked;
#else
            return true;
#endif
        }

        [UnityEditor.Callbacks.DidReloadScripts]
        static void didReload()
        {
            SessionState.SetBool("ExcludeFromBuildWindow.isEditingGroups.refreshNeeded", true);
        }

        void OnGUI()
        {
            if (EditorApplication.isCompiling)
            {
                DrawLabel("Compiling ..");
                return;
            }

            if (SessionState.GetBool("ExcludeFromBuildWindow.isEditingGroups.refreshNeeded", false))
            {
                SessionState.EraseBool("ExcludeFromBuildWindow.isEditingGroups.refreshNeeded");
                isEditingGroups = SessionState.GetBool("ExcludeFromBuildWindow.isEditingGroups", false);
            }

            try
            {
                var data = ExcludeFromBuildData.GetOrCreateData();

                if (data.CurrentGroup == null)
                {
                    DrawLabel("Huston, we have a problem!", bold: true, options: GUILayout.MinWidth(150));
                    return;
                }

                GUILayout.BeginHorizontal();
                DrawLabel("Groups", bold: true, options: GUILayout.MinWidth(150));

                GUILayout.FlexibleSpace();
                GUILayout.Label("Version " + ExcludeFromBuildSettings.Version + " ");
                if (DrawButton(" Manual ", icon: "_Help"))
                {
                    OpenManual();
                }
                if (DrawButton(" Settings ", icon: "_Popup"))
                {
                    OpenSettings();
                }
                if (DrawButton(" " +
                    "<color=#f1552c>K</color>" +
                    "<color=#f96a38>A</color>" +
                    "<color=#f98238>M</color>" +
                    "<color=#fbc43e>G</color>" +
                    "<color=#ffeb49>A</color>" +
                    "<color=#ffeb49>M</color> "))
                {
                    Application.OpenURL("https://assetstore.unity.com/publishers/37829?aid=1100lqC54&pubref=asset");
                }
                GUILayout.EndHorizontal();
                GUILayout.BeginVertical(EditorStyles.helpBox);
                DrawLabel("Each group contains a list of excluded objects and one or more build targets. The active group is the one which will be used for the next build (if it has the correct build target). Use groups to make multiple exclusion configurations.", wordwrap: true);
                GUI.enabled = !EditorApplication.isPlayingOrWillChangePlaymode;
                GUILayout.BeginHorizontal();

                // ensure currect active group for when we exit play mode (TODO: investigate)
                var lastSetGroupId = SessionState.GetInt(ExcludeFromBuildComponent.SessionActiveGroupIdKey, -1);
                if (lastSetGroupId != data.CurrentGroup.Id)
                {
                    foreach (var group in data.Groups)
                    {
                        if (group.Id == lastSetGroupId)
                        {
                            data.CurrentGroup = group;
                            break;
                        }
                    }
                }

                GUI.enabled = !IsTesting;
                var previousGroupId = data.CurrentGroup.Id;
                data.CurrentGroup = DrawGroupsToPopup(data.CurrentGroup, "Active Group (index: " + data.GetGroupIndex(data.CurrentGroup) + "): ", data.Groups);
                if (data.CurrentGroup.Id != previousGroupId)
                {
                    data.Save();
                }
                if (!isEditingGroups)
                {
                    if (DrawButton(" edit ", options: GUILayout.MaxWidth(100)))
                    {
                        isEditingGroups = true;
                    }
                }
                else
                {
                    if (DrawButton(" back ", options: GUILayout.MaxWidth(100)))
                    {
                        isEditingGroups = false;
                        EditorUtility.SetDirty(data);
                        AssetDatabase.SaveAssets();
                    }
                }
                GUI.enabled = true;
                GUILayout.EndHorizontal();
                if (!data.CurrentGroup.MatchesAllCtriteria(EditorUserBuildSettings.activeBuildTarget))
                {
                    var newGroupForBuild = data.GetFirstGroupMatchingTarget(EditorUserBuildSettings.activeBuildTarget);
                    if (newGroupForBuild == null)
                    {
                        DrawLabel(
                                "<b>Huston, we have a problem!</b>\n" +
                                "No group matches the current build settings. Please make sure at least one group matches.",
                                wordwrap: true, color: WarningTextColor, richText: true);
                    }
                    else
                    {
                        if (!data.CurrentGroup.MatchesBuildTarget(EditorUserBuildSettings.activeBuildTarget))
                        {
                            DrawLabel(
                                "The active group ('" + data.CurrentGroup.Name + "') does not include the build target " +
                                "'<b>" + EditorUserBuildSettings.activeBuildTarget + "</b>' but you are building for '<b>" + EditorUserBuildSettings.activeBuildTarget + "</b>'. " +
                                "\nThe group <b>'" + newGroupForBuild.Name + "'</b> will be used instead.",
                                wordwrap: true, color: WarningTextColor, richText: true);
                        }
                        else if (!data.CurrentGroup.MatchesDefines())
                        {
                            DrawLabel(
                                "The active group ('" + data.CurrentGroup.Name + "') defines do not match the current defines set in player settings!" +
                                "\nThe group <b>'" + newGroupForBuild.Name + "'</b> will be used instead.",
                                wordwrap: true, color: WarningTextColor, richText: true);
                        }
                        else if (!data.CurrentGroup.MatchesStandaloneSubTarget())
                        {
                            DrawLabel(
                                "The active group ('" + data.CurrentGroup.Name + "') sub target does not match the current sub target !" +
                                "\nThe group <b>'" + newGroupForBuild.Name + "'</b> will be used instead.",
                                wordwrap: true, color: WarningTextColor, richText: true);
                        }
                        else if (!data.CurrentGroup.MatchesBuildConfiguration())
                        {
                            DrawLabel(
                                "The active group ('" + data.CurrentGroup.Name + "') configuration (release/debug) does not match the current config !" +
                                "\nThe group <b>'" + newGroupForBuild.Name + "'</b> will be used instead.",
                                wordwrap: true, color: WarningTextColor, richText: true);
                        }
                    }
                }
                GUI.enabled = true;
                GUILayout.EndVertical();
                GUILayout.Space(5);

                contentScrollViewPos = EditorGUILayout.BeginScrollView(contentScrollViewPos);

                if (isEditingGroups)
                {
                    SessionState.SetBool("ExcludeFromBuildWindow.isEditingGroups", true);
                    DrawEditGroups();
                }
                else
                {
                    SessionState.SetBool("ExcludeFromBuildWindow.isEditingGroups", false);
                    DrawExcludedFilesAndDirectories();

                    // scan all
                    GUILayout.Space(15);

                    var settings = ExcludeFromBuildSettings.GetOrCreateSettings();

                    GUILayout.BeginHorizontal();
                    DrawLabel("<b>Excluded GameObjects</b> (" + (settings.ScanAllGroups ? "all groups" : data.CurrentGroup.Name) + ")", richText: true);
                    GUILayout.FlexibleSpace();
                    settings.ScanAllGroups = EditorGUILayout.ToggleLeft(new GUIContent("Scan all groups", ExcludeFromBuildSettings._ScanAllGroupsTooltip), settings.ScanAllGroups, GUILayout.MaxWidth(120));
                    var scanScenesLabel = new GUIContent("Scenes:", "Enabling this will open all scenes in the build and scan them for 'ExcludeFromBuild' components (will save and close the opened scenes).\n\nIf disabled then only the currently opened scenes are scanned.");
                    GUILayout.Label(scanScenesLabel);
                    var scanScenesPos = GUILayoutUtility.GetRect(new GUIContent("Scenes in build"), EditorStyles.popup);
                    scanScenes = (ExcludeFromBuildController.ScenesToScan)EditorGUI.EnumPopup(scanScenesPos, scanScenes);
                    if (GUILayout.Button(isScanning ? "Stop Scanning .." : " Scan Scenes "))
                    {
                        if (!isScanning)
                        {
                            StartScanning(scanScenes);
                        }
                        else
                        {
                            StopScanning();
                        }
                    }
                    /*
                    if (GUILayout.Button(" Clear Log "))
                    {
                        scanLogs.Clear();
                    }
                    */
                    GUILayout.EndHorizontal();

                    GUILayout.BeginVertical(EditorStyles.helpBox);
                    DrawLabel("Scan for GameObjects that are excluded (have an 'ExcludeFromBuild' component).\nNOTICE: Prefabs which are not used in any scene (i.e. they just exist as assets files) are ignored.", wordwrap: true);
                    GUILayout.EndVertical();

                    DrawScanResults(data, settings.ScanAllGroups);
                }
                GUILayout.FlexibleSpace();

                EditorGUILayout.EndScrollView();
            }
            catch
            {
                isScanning = false;
                scanResults = null;
                scanLogs.Clear();
                scanScenesForLastResult = ExcludeFromBuildController.ScenesToScan.OpenScenes;
                if (scanTokenSource != null)
                {
                    scanTokenSource.Cancel();
                    scanTokenSource = null;
                }
                throw;
            }
        }

        private ExcludeFromBuildData.Group DrawGroupsToPopup(ExcludeFromBuildData.Group currentPlatform, string text, List<ExcludeFromBuildData.Group> platforms)
        {
            int selectedIndex = -1;
            var options = new GUIContent[platforms.Count];
            for (int i = 0; i < platforms.Count; i++)
            {
                if (platforms[i] == currentPlatform)
                    selectedIndex = i;
                options[i] = new GUIContent(platforms[i].Name);
            }

            if (selectedIndex < 0)
                return currentPlatform;

            selectedIndex = EditorGUILayout.Popup(new GUIContent(text), selectedIndex, options);

            return platforms[selectedIndex];
        }

        private void DrawExcludedFilesAndDirectories()
        {
            var data = ExcludeFromBuildData.GetOrCreateData();
            var settings = ExcludeFromBuildSettings.GetOrCreateSettings();

            GUILayout.Space(10);

            // show list of excluded folders
            GUILayout.BeginHorizontal();
            DrawLabel("<b>Excluded files and directories</b> in '" + data.CurrentGroup.Name + "'" + (IsTesting ? " (Testing)" : ""), richText: true, color: (IsTesting ? (Nullable<Color>)Color.red : null));
            GUILayout.FlexibleSpace();
            GUI.enabled = !IsTesting;
            if (DrawButton(" Start test ", tooltip: "Hides all excludes files and folders from unity by adding '~' to the end of their path. If you are excluding scripts then this will trigger a recompile.\n\nIMPORTANT NOTICE:\nDon't forget to stop the test before commiting any changes!"))
            {
                StartTest();
            }
            GUI.enabled = true;
            if (GUILayout.Button(" Stop test "))
            {
                StopTest();
            }
            GUILayout.Space(4);
            bool _previousTestAwareBuildValue = settings.TestAwareBuild;
            settings.TestAwareBuild = GUILayout.Toggle(settings.TestAwareBuild, new GUIContent("Test aware build", ExcludeFromBuildSettings._TestAwareBuildTooltip));
            GUILayout.Space(4);
            bool _previousDelayBuildStartValue = settings.DelayBuildStart;
            settings.DelayBuildStart = GUILayout.Toggle(settings.DelayBuildStart, new GUIContent("Delay Build Start", ExcludeFromBuildSettings._DelayBuildStartTooltip));
            GUILayout.Space(4);
            GUILayout.EndHorizontal();

            GUILayout.BeginVertical(EditorStyles.helpBox);

            if (data.HasExcludedObjects)
            {
                for (int i = 0; i < data.ExcludedObjects.Count; i++)
                {
                    var excludedObj = data.ExcludedObjects[i];
                    GUILayout.BeginHorizontal();
                    if (excludedObj.IsAsset)
                    {
                        var fileName = Path.GetFileName(excludedObj.AssetPath);
                        var filePath = excludedObj.AssetPath.Replace(fileName, "");
                        DrawLabel(WrapInRichTextColor(filePath, new Color(0.6f, 0.6f, 0.6f)) + fileName, wordwrap: true, icon: excludedObj.MiniThumbnail);

                        if (GUILayout.Button(" Remove ", GUILayout.Width(90)))
                        {
                            data.Include(excludedObj);
                            data.Save();
                            i++;
                        }
                        GUI.enabled = !IsTesting;
                        if (DrawButton(" Go to ", icon: "Animation.Play", options: GUILayout.Width(70)))
                        {
                            var path = AssetDatabase.GUIDToAssetPath(excludedObj.AssetGUID);
                            var obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
                            EditorGUIUtility.PingObject(obj);
                        }
                        GUI.enabled = true;
                    }
                    GUILayout.EndHorizontal();
                }
            }
            else
            {
                DrawLabel("No files or directories are currently excluded from the build.");
            }
            GUILayout.EndVertical();

            // remove all
            if (data.HasExcludedObjects)
            {
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Remove all"))
                {
                    RemoveFilesAndDirs();
                }
                GUILayout.EndHorizontal();
            }

            // save settings if change
            if (_previousTestAwareBuildValue != settings.TestAwareBuild
                || _previousDelayBuildStartValue != settings.DelayBuildStart)
            {
                EditorUtility.SetDirty(settings);
                AssetDatabase.SaveAssets();
            }
        }

        private void DrawScanResults(ExcludeFromBuildData data, bool scanAllGroups)
        {
            BeginHorizontalIndent(10);

            // logs
            if (scanLogs.Count > 0)
            {
                scanLogsFoldout = EditorGUILayout.Foldout(scanLogsFoldout || isScanning, "Logs");
                if (scanLogsFoldout)
                {
                    GUILayout.BeginVertical(EditorStyles.helpBox);
                    scanLogScrollPos = GUILayout.BeginScrollView(isScanning ? new Vector2(0, scanLogs.Count * 17) : scanLogScrollPos, GUILayout.Height(200));
                    foreach (var line in scanLogs)
                    {
                        DrawLabel(line, richText: false);
                    }
                    GUILayout.EndScrollView();
                    GUILayout.EndVertical();
                }
            }

            if (scanResults == null)
            {
                EndHorizontalIndent(bothSides: true);
                return;
            }

            GUILayout.Space(10);

            // show list of excluded components/objects
            DrawLabel("<b>Scan Results</b> (" + scanScenesForLastResult.ToString() + ")");

            GUILayout.BeginVertical(EditorStyles.helpBox);

            foreach (var excludedObj in scanResults)
            {
                if (!excludedObj.IsAsset)
                {
                    // Skip if it does not match the scan group setting
                    if (!scanAllGroups)
                    {
                        if (!excludedObj.ComponentAllGroups && !excludedObj.ComponentGroupIds.Contains(data.CurrentGroup.Id))
                            continue;
                    }

                    GUILayout.BeginHorizontal();

                    string compName = Path.GetFileName(excludedObj.Path);
                    string compPath = excludedObj.Path.Replace(compName, "");
                    compPath = compPath.Replace("/", " / ");
                    compPath = WrapInRichTextColor(compPath, EditorGUIUtility.isProSkin ? new Color(0.8f, 0.8f, 0.8f) : new Color(0.5f, 0.5f, 0.5f));
                    string sceneName = WrapInRichTextColor(excludedObj.ComponentSceneName, EditorGUIUtility.isProSkin ? new Color(0.6f, 0.6f, 0.6f) : new Color(0.3f, 0.3f, 0.3f));
                    // Groups info tooltip
                    bool compIsPartOfAnyGroup = false;
                    string compGroups = "";
                    if (excludedObj.ComponentAllGroups)
                    {
                        compGroups = "All Groups";
                        compIsPartOfAnyGroup = true;
                    }
                    else
                    {
                        if (excludedObj.ComponentGroupIds != null)
                        {
                            if (excludedObj.ComponentGroupIds.Count > 0)
                                compIsPartOfAnyGroup = true;

                            foreach (var groupId in excludedObj.ComponentGroupIds)
                            {
                                var group = data.GetGroupById(groupId);
                                if (group != null)
                                {
                                    if (string.IsNullOrEmpty(compGroups))
                                    {
                                        compGroups += group.Name;
                                    }
                                    else
                                    {
                                        compGroups += ", " + group.Name;
                                    }
                                }
                            }
                        }
                    }
                    if (!compIsPartOfAnyGroup)
                    {
                        compGroups = "NONE (it will not be excluded)";
                        compName = "<color=red>" + compName + "</color>";
                    }
                    DrawLabel(compGroups, tooltip: "Is active for groups: " + compGroups, bold: true, options: new GUILayoutOption[] { GUILayout.MaxWidth(100) });
                    DrawLabel(sceneName + " <b>:</b> " + compPath + "<b>" + compName + "</b>", tooltip: "Is active for groups: " + compGroups);
                    if (DrawButton(" Scene ", icon: "Animation.Play", options: GUILayout.Width(70)))
                    {
                        var path = AssetDatabase.GUIDToAssetPath(excludedObj.ComponentSceneGUID);
                        var obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
                        EditorGUIUtility.PingObject(obj);
                    }

                    GUILayout.EndHorizontal();
                }
            }

            GUILayout.EndVertical();
            EndHorizontalIndent(bothSides: true);

            GUILayout.Space(15);
        }

        private void DrawEditGroups()
        {
            GUILayout.Space(10);

            // show list of groups
            var data = ExcludeFromBuildData.GetOrCreateData();
            DrawLabel("<b>Groups</b> (active: '" + data.CurrentGroup.Name + "')", richText: true);

            string currentBuiltTargetName = EditorUserBuildSettings.activeBuildTarget.ToString();
            if (data.HasGroups)
            {
                GUILayout.BeginHorizontal();
                DrawUtils.DrawLabel(new GUIContent("Name"), null, GUILayout.Width(185));
                DrawUtils.DrawLabel(new GUIContent("Build Target"), null, GUILayout.Width(160));
                DrawUtils.DrawLabel(new GUIContent("Sub Target", "Select which standalone sub target (useful for headless builds). 'Any' means the sub target will not be checked."), null, GUILayout.Width(80));
                DrawUtils.DrawLabel(new GUIContent("Debug", "Choose whether this group should only be used for a certain build configuration (release or development/debug)."), null, GUILayout.Width(70));
                DrawUtils.DrawLabel(new GUIContent("Actions"), null);
                GUILayout.EndHorizontal();

                var matchingGroup = data.GetFirstGroupMatchingTarget(EditorUserBuildSettings.activeBuildTarget);

                for (int i = 0; i < data.Groups.Count; i++)
                {
                    var group = data.Groups[i];

                    var boxStyle = new GUIStyle(EditorStyles.helpBox);
                    if (group == matchingGroup)
                        boxStyle.normal.background = DrawUtils.GetColorTexture0(new Color(0f, 1f, 0f, 0.05f));
                    GUILayout.BeginVertical(boxStyle);

                    GUILayout.BeginHorizontal();

                    // Name
                    var style = new GUIStyle(GUI.skin.textField);
                    if (group == data.CurrentGroup)
                        style.fontStyle = FontStyle.Bold;
                    group.Name = EditorGUILayout.TextField(group.Name, style, GUILayout.ExpandWidth(true), GUILayout.Width(185));

                    // Built Target
                    var col = GUI.backgroundColor;
                    var groupMultiSelectLabelColor = group.MatchesBuildTarget(EditorUserBuildSettings.activeBuildTarget) ? new Color(0.7f, 1f, 0.7f) : new Color(1f, 0.7f, 0.7f);
                    GUI.backgroundColor = groupMultiSelectLabelColor;
                    var groupMultiSelectTooltip = group.MatchesBuildTarget(EditorUserBuildSettings.activeBuildTarget) ? "This group contains the current build target('" + currentBuiltTargetName + "')." : " This group does NOT contain the current build target ('" + currentBuiltTargetName + "').";
                    group.BuildTargets = DrawUtils.DrawBuildTargetsMultiSelect(group.BuildTargets, "", "Select which build targets (platforms) this group should be used for.\n\n" + groupMultiSelectTooltip, groupMultiSelectLabelColor, drawLabel: false, GUILayout.Width(160));
                    GUI.backgroundColor = col;

                    // SubTarget
                    var standaloneMultiSelectLabelColor = group.MatchesStandaloneSubTarget() ? new Color(0.7f, 1f, 0.7f) : new Color(1f, 0.7f, 0.7f);
                    GUI.backgroundColor = standaloneMultiSelectLabelColor;
                    group.StandaloneSubTarget = DrawUtils.DrawStandaloneSubTargetMultiSelect(group.StandaloneSubTarget, null , standaloneMultiSelectLabelColor, drawLabel: false, GUILayout.Width(80));
                    GUI.backgroundColor = col;

                    // Debug or Release?
                    var configMultiSelectLabelColor = group.MatchesBuildConfiguration() ? new Color(0.7f, 1f, 0.7f) : new Color(1f, 0.7f, 0.7f);
                    GUI.backgroundColor = configMultiSelectLabelColor;
                    group.BuildConfiguration = (ExcludeFromBuildData.BuildConfiguration)EditorGUILayout.EnumPopup(group.BuildConfiguration, GUILayout.Width(70));
                    GUI.backgroundColor = col;

                    GUI.enabled = group != data.CurrentGroup;
                    if (DrawButton(" Delete "))
                    {
                        Undo.RegisterCompleteObjectUndo(data, "Delete platform");
                        data.Groups.RemoveAt(i);
                        data.Save();
                        GUI.enabled = true;
                        GUILayout.EndHorizontal();
                        continue;
                    }
                    GUI.enabled = true;
                    if (DrawButton(" Copy "))
                    {
                        Undo.RegisterCompleteObjectUndo(data, "Copy platform");
                        var newGroup = new ExcludeFromBuildData.Group(data.GetNextGroupId(), group);
                        newGroup.Name = newGroup.Name + " Copy";
                        data.Groups.Add(newGroup);
                        data.Save();
                        GUI.enabled = true;
                        GUILayout.EndHorizontal();
                        continue;
                    }
                    GUI.enabled = group != data.CurrentGroup;
                    if (DrawButton(" Activate "))
                    {
                        data.CurrentGroup = group;
                        data.Save();
                    }
                    GUI.enabled = true;

                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    // Index
                    GUILayout.Label("idx: " + i, GUILayout.Width(40));
                    // Spacer
                    GUILayout.Space(145);
                    // Defines
                    var definesLabelColor = group.MatchesDefines() ? new Color(0.7f, 1f, 0.7f) : new Color(1f, 0.7f, 0.7f);
                    GUI.backgroundColor = definesLabelColor;
                    DrawUtils.DrawLabel(new GUIContent("Defines:", "Defines are combined with AND logic. All specified defines need to exists for this to be a positive match."), definesLabelColor, GUILayout.Width(55));
                    group.Defines = GUILayout.TextField(group.Defines, GUILayout.ExpandWidth(true));
                    GUI.backgroundColor = col;
                    GUILayout.EndHorizontal();

                    GUILayout.EndVertical();

                    GUILayout.Space(15);
                }
            }
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (DrawButton(" Activate first matching group ", "Searches all groups and activates the first one that matches the current build target (" + currentBuiltTargetName + ")."))
            {
                data.CurrentGroup = data.GetFirstGroupMatchingTarget(EditorUserBuildSettings.activeBuildTarget);
                data.Save();
            }
            if (DrawButton(" Add new group"))
            {
                Undo.RegisterCompleteObjectUndo(data, "Delete platform");
                var group = new ExcludeFromBuildData.Group(data.GetNextGroupId(), currentBuiltTargetName, EditorUserBuildSettings.activeBuildTarget);
                data.Groups.Add(group);
                data.CurrentGroup = group;
                data.Save();
            }

            GUILayout.EndHorizontal();
            if (DrawButton("Save & Back"))
            {
                isEditingGroups = false;
                data.Save();
            }
        }

        public static void OpenManual()
        {
            EditorUtility.OpenWithDefaultApp("Assets/ExcludeFromBuild/ExcludeFromBuildManual.pdf");
        }

        public void OpenSettings()
        {
            ExcludeFromBuildSettings.SelectSettings();
        }

        public void RemoveFilesAndDirs()
        {
            ExcludeFromBuildController.ClearFilesAndDirs();
        }

        public void StartTest()
        {
            IsTesting = true;
            bool succeeded = ExcludeFromBuildController.ApplyExcludedFileAndDirNames();
            if (!succeeded)
            {
                IsTesting = false;
                EditorUtility.DisplayDialog("Starting TEST FAILED", "Errors will be logged in the console.\n\nIf you need more detailed information then please set the 'Log Level' to 'Log' in the settings and try again.", "Damn");
            }
        }

        public void StopTest()
        {
            IsTesting = false;
            ExcludeFromBuildController.RevertExcludedFileAndDirNames();
        }

        public async void StartScanning(ExcludeFromBuildController.ScenesToScan scenesToScan)
        {
            if (scanTokenSource != null && !scanTokenSource.IsCancellationRequested)
                scanTokenSource.Cancel();

            isScanning = true;
            scanResults = null;
            scanLogs.Clear();
            scanLogScrollPos = Vector2.zero;
            scanLogsFoldout = false;
            scanScenesForLastResult = scenesToScan;

            try
            {
                scanTokenSource = new System.Threading.CancellationTokenSource();
                var ct = scanTokenSource.Token;

                await Task.Delay(100);
                await ExcludeFromBuildController.ScanScenesForExcludedGameObjects(ct, scenesToScan, scanLogs, onScanComplete);
                Repaint();
            }
            finally
            {
                isScanning = false;
                scanLogsFoldout = false;
            }
        }

        public void StopScanning()
        {
            if (scanTokenSource != null && !scanTokenSource.IsCancellationRequested)
                scanTokenSource.Cancel();
        }

        void onScanComplete(List<ExcludedObject> results)
        {
            scanResults = results;
        }

        public static void DrawLabel(string text, Color? color = null, bool bold = false, bool wordwrap = true, bool richText = true, Texture icon = null, string tooltip = null, params GUILayoutOption[] options)
        {
            if (!color.HasValue)
                color = GUI.skin.label.normal.textColor;

            var style = new GUIStyle(GUI.skin.label);
            if (bold)
                style.fontStyle = FontStyle.Bold;

            style.normal.textColor = color.Value;
            style.wordWrap = wordwrap;
            style.richText = richText;
            style.imagePosition = ImagePosition.ImageLeft;

            var content = new GUIContent();
            content.text = text;
            if (tooltip != null)
            {
                content.tooltip = tooltip;
            }
            if (icon != null)
            {
                GUILayout.Space(16);
                var position = GUILayoutUtility.GetRect(content, style);
                GUI.DrawTexture(new Rect(position.x - 16, position.y, 16, 16), icon);
                GUI.Label(position, content, style);
            }
            else
            {
                GUILayout.Label(content, style, options);
            }
        }

        public static void DrawSelectableLabel(string text, Color? color = null, bool bold = false, bool wordwrap = true, bool richText = true)
        {
            if (!color.HasValue)
                color = GUI.skin.label.normal.textColor;

            var style = new GUIStyle(GUI.skin.label);
            if (bold)
                style.fontStyle = FontStyle.Bold;
            style.normal.textColor = color.Value;
            style.wordWrap = wordwrap;
            style.richText = richText;

            var content = new GUIContent(text);
            var position = GUILayoutUtility.GetRect(content, style);
            EditorGUI.SelectableLabel(position, text, style);
        }

        public static bool DrawButton(string text, string tooltip = null, string icon = null, params GUILayoutOption[] options)
        {
            GUIContent content;

            // icon
            if (!string.IsNullOrEmpty(icon))
                content = EditorGUIUtility.IconContent(icon);
            else
                content = new GUIContent();

            // text
            content.text = text;

            // tooltip
            if (!string.IsNullOrEmpty(tooltip))
                content.tooltip = tooltip;

            var style = new GUIStyle(GUI.skin.button);
            style.richText = true;

            return GUILayout.Button(content, style, options);
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

        public static string WrapInRichTextColor(string text, Color color)
        {
            var hexColor = ColorUtility.ToHtmlStringRGB(color);
            return "<color=#" + hexColor + ">" + text + "</color>";
        }
    }
}
