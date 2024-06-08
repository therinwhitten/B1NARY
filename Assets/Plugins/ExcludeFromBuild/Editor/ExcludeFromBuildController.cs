using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using System.Threading.Tasks;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using System.Threading;
using System.Linq;

namespace Kamgam.ExcludeFromBuild
{
    public enum LogLevel
    {
        Log = 0,
        Warning = 1,
        Error = 2,
        Message = 3,
        NoLogs = 99
    }

    public delegate void LogCallback(string message, LogLevel logLevel = LogLevel.Log);

    public class ExcludeFromBuildController :
        IPreprocessBuildWithReport,
        IProcessSceneWithReport
    {
        public enum ScenesToScan { OpenScenes, ScenesInBuild, AllScenes }

        public int callbackOrder => int.MinValue + 1;

        public const string HiddenAssetsDirName = ".EFB_HiddenAssets~";

        public static void LogMessage(string message, LogLevel logLevel = LogLevel.Log)
        {
            var settingsLogLevel = ExcludeFromBuildSettings.GetOrCreateSettings().LogLevel;

            if (logLevel < settingsLogLevel)
                return;

            switch (logLevel)
            {
                case LogLevel.Log:
                    Debug.Log("ExcludeFromBuild: " + message);
                    break;
                case LogLevel.Warning:
                    Debug.LogWarning("ExcludeFromBuild: " + message);
                    break;
                case LogLevel.Error:
                    Debug.LogError("ExcludeFromBuild: " + message);
                    break;
                case LogLevel.Message:
                    Debug.Log("ExcludeFromBuild: " + message);
                    break;
                default:
                    break;
            }
        }

        [InitializeOnLoadMethod]
        public static void InitializeOnLoad()
        {
            EditorApplication.playModeStateChanged += onPlayModeStateChanged;
        }

        private static void onPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode && EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Init();
            }
        }

        public static void Init()
        {
            var data = ExcludeFromBuildData.GetOrCreateData();
            if (data)
            {
                var group = data.CurrentGroup;
                data.CurrentGroup = group; // This initializes the group ids in the session store (important for the Component).
                LogMessage("Entering play mode with group '" + group.Name + "'", LogLevel.Log);
            }
        }

        public static void ClearFilesAndDirs()
        {
            var data = ExcludeFromBuildData.GetOrCreateData();
            if (data)
            {
                data.Clear();
                data.Save();
            }
        }

        [MenuItem("Assets/Exclude From Build", priority = 1100)]
        public static void AddSelectionToExclusions()
        {
            var data = ExcludeFromBuildData.GetOrCreateData();
            var settings = ExcludeFromBuildSettings.GetOrCreateSettings();
            bool isExcluded = isSelectionExcluded();


            bool modifyScenesInBuild = false;
            if (settings.SceneFilesBehaviour == ExcludeFromBuildSettings.SceneFileBehaviour.AlwaysAsk)
            {
                SceneAsset[] scenes = Selection.GetFiltered<SceneAsset>(SelectionMode.Assets);
                if (scenes != null && scenes.Length > 0)
                {
                    modifyScenesInBuild = EditorUtility.DisplayDialog((isExcluded ? "Enable" : "Disable") + " scenes in Build Settings?", "There are some scene files in your selection. Would you like to " + (isExcluded ? "enable" : "disable") + " them in the Build Settings too?\n\nHint: You can control this dialog in the settings.", "Yes", "No");
                }
            }
            else if (settings.SceneFilesBehaviour == ExcludeFromBuildSettings.SceneFileBehaviour.AlwaysUpdateBuildSettings)
            {
                modifyScenesInBuild = true;
            }

            Undo.RegisterCompleteObjectUndo(data, "Add or remove objects from build.");
            bool selectionAddsStreamingAssets = false;
            foreach (UnityEngine.Object obj in Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets))
            {
                if (isExcluded)
                    RemoveFromExclusion(obj);
                else
                    AddToExclusion(obj);

                /// check if streaming asset
                if (!isExcluded && !selectionAddsStreamingAssets)
                {
                    string path = AssetDatabase.GetAssetPath(obj);
                    if (!string.IsNullOrEmpty(path) && path.Contains("/StreamingAssets"))
                        selectionAddsStreamingAssets = true;
                }


                // handle scene assets (add/remove them from the build)
                if (modifyScenesInBuild)
                {
                    var scene = obj as SceneAsset;
                    if (scene != null)
                    {
                        var scenePath = AssetDatabase.GetAssetPath(obj);
                        var scenesInBuildSettings = EditorBuildSettings.scenes;
                        foreach (var sceneSetting in scenesInBuildSettings)
                        {
                            if (sceneSetting.path == scenePath)
                                sceneSetting.enabled = isExcluded;
                        }
                        EditorBuildSettings.scenes = scenesInBuildSettings;
                    }
                }
            }

            ExcludeFromBuildData.GetOrCreateData().Save();

            repaintWindow();

            // hint about StreamingAssets for the first added streaming asset
            if (!isExcluded && selectionAddsStreamingAssets)
            {
                int numOfStreamingAssets = data.ExcludedObjects.Count(o => o.Path.Contains("StreamingAssets"));
                if (numOfStreamingAssets == 1)
                {
                    bool openManual = !EditorUtility.DisplayDialog("IMPORTANT message on StreamingAssets", "TLDR:\nStreamingAssets are excluded just fine but may still show up in build reports.\n\nLONG READ:\nThis message is for those of you who are using a Build Report tool to check the size after the build.\n\nIf you do then please notice that the files in /StreamingAssets may still be listed even if you exclude them from the build.\n\nRest assured that they are in fact excluded, no matter what the Build Report tells you!\n\nThere is a more detailed explanation in the Manual as to why that is.\n\nSorry for the inconvenience.", "Okay, got it", "Open Manual");
                    if (openManual)
                    {
                        ExcludeFromBuildWindow.OpenManual();
                    }
                }
            }
        }

        // We are hijacking the validate method to do the IsExcluded lookup on demand for each right-click.
        [MenuItem("Assets/Exclude From Build", isValidateFunction: true)]
        static bool ValidateAddSelectionToExclusions()
        {
            if (Selection.objects.Length == 0)
                return false;

            bool isExcluded = isSelectionExcluded();
            Menu.SetChecked("Assets/Exclude From Build", isExcluded);

            repaintWindow();

            return true;
        }

        static void repaintWindow()
        {
            if (EditorWindow.HasOpenInstances<ExcludeFromBuildWindow>())
            {
                var window = ExcludeFromBuildWindow.GetOrOpen(focus: false);
                window.Repaint();
            }
        }

        static bool isSelectionExcluded()
        {
            var data = ExcludeFromBuildData.GetOrCreateData();

            foreach (UnityEngine.Object obj in Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets))
            {
                var path = AssetDatabase.GetAssetPath(obj);
                if (!string.IsNullOrEmpty(path))
                {
                    var guid = AssetDatabase.AssetPathToGUID(path);
                    if (!string.IsNullOrEmpty(guid) && data.IsExcluded(guid))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static void AddToExclusion(Object obj)
        {
            var data = ExcludeFromBuildData.GetOrCreateData();
            var settings = ExcludeFromBuildSettings.GetOrCreateSettings();

            var path = AssetDatabase.GetAssetPath(obj);
            if (path == "Assets")
            {
                LogMessage("Excluding the whole /Assets folder is not such a good idea :D", LogLevel.Warning);
                return;
            }
            if (path.EndsWith("/Editor") || path.EndsWith("\\Editor") || path.Contains("/Editor/") || path.Contains("\\Editor\\"))
            {
                LogMessage("Editor folders are not included in the build anyways. Ignoring.", LogLevel.Warning);
                return;
            }

            if (obj == data)
            {
                LogMessage("Sorry, no can do. The data is not part of any build but it is needed to revert the hidden files after the build.", LogLevel.Warning);
                return;
            }
            if (obj == settings)
            {
                LogMessage("Sorry, no can do. The settings are not part of any build but they are needed for the tool to work.", LogLevel.Warning);
                return;
            }
            if ((path.EndsWith("/ExcludeFromBuild") || path.Contains("/ExcludeFromBuild/")) && !path.Contains("/Examples"))
            {
                LogMessage("Sorry, but you should not exclude the tool itself. This would break reverting the files back after the build. None of the files in there are part of the build.", LogLevel.Warning);
                return;
            }

            if (!string.IsNullOrEmpty(path))
            {
                var guid = AssetDatabase.AssetPathToGUID(path);
                if (!string.IsNullOrEmpty(guid))
                {
                    data.Exclude(guid, path);
                    LogMessage("Added '" + path + "' to excluded objects (group: " + data.CurrentGroup.Name + ").", LogLevel.Log);
                }
                else
                {
                    LogMessage("Failed to get GUID for '" + path + "'. Will be ignored.", LogLevel.Log);
                }
            }
        }

        public static void RemoveFromExclusion(Object obj)
        {
            var data = ExcludeFromBuildData.GetOrCreateData();

            var path = AssetDatabase.GetAssetPath(obj);
            if (!string.IsNullOrEmpty(path))
            {
                var guid = AssetDatabase.AssetPathToGUID(path);
                if (!string.IsNullOrEmpty(guid))
                {
                    data.Remove(guid);
                    LogMessage("Removed '" + path + "' from excluded objects (group: " + data.CurrentGroup.Name + ").", LogLevel.Log);
                }
                else
                {
                    LogMessage("Failed to get GUID for '" + path + "'.", LogLevel.Log);
                }
            }
        }

        public static string GetSelectedPathOrFallback()
        {
            string path = "Assets";

            foreach (UnityEngine.Object obj in Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets))
            {
                path = AssetDatabase.GetAssetPath(obj);
                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                {
                    path = Path.GetDirectoryName(path);
                    break;
                }
            }
            return path;
        }


        public static async Task ScanScenesForExcludedGameObjects(CancellationToken ct, ScenesToScan scenesToScan, List<string> logLines, System.Action<List<ExcludedObject>> onComplete)
        {
            var results = new List<ExcludedObject>();

            try
            {
                if (scenesToScan == ScenesToScan.AllScenes)
                {
                    // warn user
                    bool proceed = EditorUtility.DisplayDialog("Warning", "You have checked 'all scenes'. The scan process will have to close your scenes.", "Ok", "Cancel");
                    if (!proceed)
                    {
                        onComplete(results);
                        return;
                    }

                    proceed = EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                    if (!proceed)
                    {
                        onComplete(results);
                        return;
                    }
                }

                // Always scan open scenes
                await logScanMessage("Scanning open scenes ..", logLines);
                var openendScenes = new List<string>();
                for (int i = 0; i < EditorSceneManager.sceneCount; i++)
                {
                    if (ct.IsCancellationRequested)
                        break;

                    var scene = EditorSceneManager.GetSceneAt(i);
                    await logScanMessage("  Scanning scene " + scene.path, logLines);
                    ScanSceneAndAddToResults(ct, scene, results);
                    openendScenes.Add(scene.path);
                }

                // scenes in build
                if (scenesToScan == ScenesToScan.ScenesInBuild)
                {
                    // close open scenes
                    await logScanMessage("Closing open scenes.", logLines);
                    for (int i = 1; i < EditorSceneManager.sceneCount; i++)
                    {
                        if (ct.IsCancellationRequested)
                            break;

                        var scene = EditorSceneManager.GetSceneAt(i);
                        EditorSceneManager.CloseScene(scene, true);
                    }

                    // scan scenes enabled for build
                    await logScanMessage("Scanning enabled scenes in build ..", logLines);
                    List<string> scenes = new List<string>();
                    foreach (var sceneSettings in EditorBuildSettings.scenes)
                    {
                        if (ct.IsCancellationRequested)
                            break;

                        if (sceneSettings.enabled)
                        {
                            var scene = EditorSceneManager.OpenScene(sceneSettings.path, UnityEditor.SceneManagement.OpenSceneMode.Single);
                            await logScanMessage("  Scanning scene " + sceneSettings.path, logLines);
                            if (openendScenes.Contains(sceneSettings.path))
                                continue;

                            ScanSceneAndAddToResults(ct, scene, results);
                        }
                    }

                    // reopen scenes
                    await logScanMessage("Reopening closed scenes.", logLines);
                    bool first = true;
                    foreach (var path in openendScenes)
                    {
                        if (ct.IsCancellationRequested)
                            break;

                        EditorSceneManager.OpenScene(path, first ? OpenSceneMode.Single : OpenSceneMode.Additive);
                        first = false;
                    }
                    await logScanMessage("Done.", logLines);
                }

                // all scenes
                else if (scenesToScan == ScenesToScan.AllScenes)
                {
                    await logScanMessage("Searching for scene files.", logLines);
                    var sceneGUIDs = AssetDatabase.FindAssets("t:scene");
                    if (sceneGUIDs.Length > 0)
                    {
                        // close open scenes
                        await logScanMessage("Closing open scenes.", logLines);
                        for (int i = 1; i < EditorSceneManager.sceneCount; i++)
                        {
                            if (ct.IsCancellationRequested)
                                break;

                            var scene = EditorSceneManager.GetSceneAt(i);
                            EditorSceneManager.CloseScene(scene, true);
                        }

                        await logScanMessage("Scanning scenes ..", logLines);
                        foreach (var sceneGUID in sceneGUIDs)
                        {
                            if (ct.IsCancellationRequested)
                                break;

                            string scenePath = AssetDatabase.GUIDToAssetPath(sceneGUID);
                            try
                            {
                                if (scenePath.Contains("Packages"))
                                {
                                    // see: https://forum.unity.com/threads/check-if-asset-inside-package-is-readonly.900902/
                                    var packageInfo = UnityEditor.PackageManager.PackageInfo.FindForAssetPath(scenePath);
                                    if (packageInfo != null
                                        && packageInfo.source != UnityEditor.PackageManager.PackageSource.Local
                                        && packageInfo.source != UnityEditor.PackageManager.PackageSource.Embedded)
                                    {
                                        await logScanMessage("  Skipping '" + scenePath + "' (it's read-only).", logLines);
                                        continue;
                                    }
                                }

                                var scene = EditorSceneManager.OpenScene(scenePath, UnityEditor.SceneManagement.OpenSceneMode.Single);
                                await logScanMessage("  Scanning scene " + scene.path, logLines);
                                if (openendScenes.Contains(scene.path))
                                    continue;

                                ScanSceneAndAddToResults(ct, scene, results);
                            }
                            catch (System.Exception e)
                            {
                                await logScanMessage("  Error ('" + e.Message + "') while opening '" + scenePath + "'. Skipping.", logLines);
                            }
                        }
                    }

                    // reopen scenes
                    await logScanMessage("Reopening closed scenes.", logLines);
                    bool first = true;
                    foreach (var path in openendScenes)
                    {
                        EditorSceneManager.OpenScene(path, first ? OpenSceneMode.Single : OpenSceneMode.Additive);
                        first = false;
                    }
                    await logScanMessage("Done.", logLines);
                }
            }
            /*catch(System.Exception e)
            {
                logLines.Add("ERROR: " + e.Message);
            }*/
            finally
            {
                onComplete(results);
            }
        }

        public static async Task logScanMessage(string msg, List<string> logLines)
        {
            logLines.Add(msg);
            await Task.Delay(10);
        }

        public static void ScanSceneAndAddToResults(CancellationToken ct, Scene scene, List<ExcludedObject> results)
        {
            string sceneGUID = AssetDatabase.AssetPathToGUID(scene.path);
            var roots = scene.GetRootGameObjects();
            foreach (var root in roots)
            {
                if (ct.IsCancellationRequested)
                    return;

                var comps = root.GetComponentsInChildren<ExcludeFromBuildComponent>(includeInactive: true);
                foreach (var comp in comps)
                {
                    if (ct.IsCancellationRequested)
                        return;

                    var obj = new ExcludedObject(sceneGUID, scene.name, comp.GUID, comp.GetPath(), new List<int>(comp.GroupIds), comp.AllGroups);
                    results.Add(obj);
                }
            }
        }

        public void OnPreprocessBuild(BuildReport report)
        {
            // Memorize the build options so we can recover them even after a recompile.
            SessionState.SetInt("ExcludeFromBuild.PlayerOptions", (int)report.summary.options);

            preprocessBuild();
            delayBuildStartIfNeeded();

            // We have to do it this way because IPostprocessBuildWithReport is not fired if the build fails: 
            // see: https://forum.unity.com/threads/ipostprocessbuildwithreport-and-qa-embarrasing-answer-about-a-serious-bug.891055/
            waitForBuildCompletion(report);
        }

        static int _crossCompileBuildRestartId = -1;

        protected void delayBuildStartIfNeeded()
        {
            // Clean bool if not needed.
            if (!ExcludeFromBuildSettings.GetOrCreateSettings().DelayBuildStart)
                SessionState.EraseBool("DontDelayBuildAgain");

            // Don't aboert if testing and test-aware is on (in that case the files are already excluded)
            if (ExcludeFromBuildSettings.GetOrCreateSettings().TestAwareBuild && ExcludeFromBuildWindow.IsTesting)
                return;

            if (!ExcludeFromBuildSettings.GetOrCreateSettings().DelayBuildStart || SessionState.GetBool("DontDelayBuildAgain", false))
                return;

            LogMessage("Aborting build du to 'DelayBuildStart = true' with fake error.");

            // Make sure either the delay or the cross compile callback will trigger a new build.
            SessionState.SetBool("DontDelayBuildAgain", true);
            DelayedCallback.Create(scheduleBuildRestart, ExcludeFromBuildSettings.GetOrCreateSettings().BuildStartDelayInSec, logCountdown: true);
            _crossCompileBuildRestartId = CrossCompileCallbacks.RegisterCallback(restartBuildAfterCompilation);

            throw new BuildFailedException("This is NO ERROR. All is fine. The build was aborted because 'Delay Build Start' is enabled in ExcludeFromBuild. It will be restarted momentarily.");
        }

        static void restartBuildAfterCompilation()
        {
            LogMessage("Recompile detected");
            DelayedCallback.Create(scheduleBuildRestart, ExcludeFromBuildSettings.GetOrCreateSettings().BuildStartDelayInSec, logCountdown: true);
        }

        static void scheduleBuildRestart()
        {
            if (_crossCompileBuildRestartId >= 0)
            {
                CrossCompileCallbacks.Clear(_crossCompileBuildRestartId);
            }

            ConditionCallback.Create(restartBuild, canRestartBuild);
        }

        static bool canRestartBuild()
        {
            return !EditorApplication.isCompiling && !BuildPipeline.isBuildingPlayer;
        }

        static void restartBuild()
        {
            LogMessage("Restarting Build");

            var options = GetBuildPlayerOptions();

            int optionsAsInt = SessionState.GetInt("ExcludeFromBuild.PlayerOptions", -1);
            if (optionsAsInt >= 0)
            {
                options.options = (BuildOptions)optionsAsInt;
            }
            BuildPipeline.BuildPlayer(options);
        }

        static BuildPlayerOptions GetBuildPlayerOptions(bool askForLocation = false, BuildPlayerOptions defaultOptions = new BuildPlayerOptions())
        {
            System.Reflection.MethodInfo method = typeof(BuildPlayerWindow.DefaultBuildMethods).GetMethod(
                "GetBuildPlayerOptionsInternal",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static
            );

            // Fallback in case reflections did not work
            if (method == null)
            {
                BuildPlayerWindow.DefaultBuildMethods.GetBuildPlayerOptions(defaultOptions);
                return defaultOptions;
            }

            // invoke internal method
            return (BuildPlayerOptions)method.Invoke(
                null,
                new object[] { askForLocation, defaultOptions }
                );
        }

        const string PreBuildGroupSessionKey = "Kamgam.ExcludeFromBuild.PreBuildGroupId";
        const string PreBuildGroupActuallyExcludedSessionKey = "Kamgam.ExcludeFromBuild.PreBuildGroupResolvedId";

        static void preprocessBuild()
        {
            // Warn if the current build targets to not align
            var data = ExcludeFromBuildData.GetOrCreateData();
            SessionState.SetInt(PreBuildGroupSessionKey, data.CurrentGroup.Id);

            if (!data.CurrentGroup.MatchesAllCtriteria(EditorUserBuildSettings.activeBuildTarget))
            {
                var newGroup = data.GetFirstGroupMatchingTarget(EditorUserBuildSettings.activeBuildTarget);
                if (newGroup == null)
                {
                    LogMessage("No group with build target '" + EditorUserBuildSettings.activeBuildTarget + "' found. No files will be excluded.", LogLevel.Warning);
                    return;
                }
                else
                {
                    LogMessage("Switching active group to '" + newGroup.Name + "' because the build targets of the active group do not contain '" + EditorUserBuildSettings.activeBuildTarget + "' or does not match defines or does not match sub targets or does not match release/debug config.", LogLevel.Warning);
                    data.CurrentGroup = newGroup;
                }
            }
            // Always log this
            LogMessage("Building with active group '" + data.CurrentGroup.Name + "'.", LogLevel.Message);

            // If TestAwareBuild is active and a test is running then don't execute the exclude logic.
            var settings = ExcludeFromBuildSettings.GetOrCreateSettings();
            if (!(settings.TestAwareBuild && ExcludeFromBuildWindow.IsTesting))
                ApplyExcludedFileAndDirNames();
        }

        public static bool ApplyExcludedFileAndDirNames()
        {
            bool succeeded = true;

            try
            {
                string projectDir = Path.GetFullPath(Path.Combine(Application.dataPath, "../")).Replace("\\", "/");
                var data = ExcludeFromBuildData.GetOrCreateData();

                LogMessage("Excluding group '" + data.CurrentGroup.Name + "'.", LogLevel.Message);
                SessionState.SetInt(PreBuildGroupActuallyExcludedSessionKey, data.CurrentGroup.Id);

                // Ensure that parent directories are handled first.
                data.SortExcludedObjects();
                foreach (var obj in data.ExcludedObjects)
                {
                    LogMessage("Moving '" + obj.AssetPath + "'.");

                    var path = projectDir + obj.AssetPath;
                    bool renameSuccess = rename(path, "~", true);
                    if (renameSuccess)
                        renameSuccess = rename(path + ".meta", "~", true);

                    if (renameSuccess)
                    {
                        LogMessage("   Successfully moved '" + obj.AssetPath + "'.");
                    }
                    else
                    {
                        succeeded = false;
                        break;
                    }
                }

                if (!succeeded)
                {
                    LogMessage("Renaming files or folders FAILED. Aborting and reverting. NOTICE: no files or folders will be excluded in this build!", LogLevel.Error);
                    RevertExcludedFileAndDirNames();
                }
                AssetDatabase.Refresh();
            }
            catch (System.Exception e)
            {
                LogMessage("An error occurred while renaming files or folders. Reverting changes. NOTICE: no files or folders will be excluded in this build!\nError: " + e.Message, LogLevel.Error);
                RevertExcludedFileAndDirNames();
                succeeded = false;
            }

            return succeeded;
        }

        public static void RevertExcludedFileAndDirNames()
        {
            string projectDir = Path.GetFullPath(Path.Combine(Application.dataPath, "../")).Replace("\\", "/");
            var data = ExcludeFromBuildData.GetOrCreateData();

            // Ensure that the correct group is being reverted.
            int excludedGroupId = SessionState.GetInt(PreBuildGroupActuallyExcludedSessionKey, -1);
            if (excludedGroupId >= 0)
            {
                SessionState.EraseInt(PreBuildGroupActuallyExcludedSessionKey);
                if (data.CurrentGroup.Id != excludedGroupId)
                {
                    data.SetCurrentGroupById(excludedGroupId);
                    LogMessage("Changing group for revert to '" + data.CurrentGroup.Name + "'.", LogLevel.Message);
                }
            }

            LogMessage("Reverting group '" + data.CurrentGroup.Name + "'.", LogLevel.Message);

            // In StreamingAssets the *~ folders and files are not ignored in
            // builds despite them being hidden in the Editor (thanks to Starbox).
            string hiddenAssetsDir = null;
            if (data.HasStreamingAssets())
            {
                hiddenAssetsDir = Application.dataPath + "/" + HiddenAssetsDirName;
                if (!Directory.Exists(hiddenAssetsDir))
                    Directory.CreateDirectory(hiddenAssetsDir);
            }

            foreach (var obj in data.ExcludedObjects)
            {
                LogMessage("Reverting '" + obj.AssetPath + "'.");

                var path = projectDir + obj.AssetPath;
                bool succeeded = rename(path, "~", false);
                if (succeeded)
                {
                    succeeded = rename(path + ".meta", "~", false);
                }

                if (succeeded)
                    LogMessage("   Successfully reverted '" + obj.AssetPath + "'.");
                else
                    LogMessage("   Error while reverting '" + path + "~" + "' to '" + path + "'.", LogLevel.Error);
            }

            try
            {
                // clean up hidden folders
                if (!string.IsNullOrEmpty(hiddenAssetsDir) && Directory.Exists(hiddenAssetsDir) && Directory.GetFiles(hiddenAssetsDir, "*", SearchOption.AllDirectories).Length == 0)
                    Directory.Delete(hiddenAssetsDir, true);
            }
            catch (System.Exception e)
            {
                LogMessage("Error while deleting '" + hiddenAssetsDir + "': " + e.Message, LogLevel.Error);
            }

            // Ensure that the correct group is active after all is set and done.
            int activeGroupId = SessionState.GetInt(PreBuildGroupSessionKey, -1);
            if (activeGroupId >= 0)
            {
                SessionState.EraseInt(PreBuildGroupSessionKey);
                if (data.CurrentGroup.Id != activeGroupId)
                {
                    data.SetCurrentGroupById(activeGroupId);
                    LogMessage("Reverting active group to '" + data.CurrentGroup.Name + "'.", LogLevel.Message);
                }
            }

            AssetDatabase.Refresh();
        }

        static bool rename(string path, string extension, bool add)
        {
            try
            {
                path = path.Replace("\\", "/");

                // Change path if it is within StreamingAssets.
                bool isStreamingAsset = path.Contains("/StreamingAssets/") || path.EndsWith("/StreamingAssets");
                string extendedPath;
                if (isStreamingAsset)
                {
                    extendedPath = path.Replace("/StreamingAssets", "/" + HiddenAssetsDirName + "/StreamingAssets");
                }
                else
                {
                    extendedPath = path + extension;
                }

                if (add)
                {
                    // exclude / hide
                    if (isStreamingAsset)
                    {
                        // Create the parent dir (necessary for StreamingAssets)
                        // Trim "at" the end to always get the parent directory, see:
                        // https://docs.microsoft.com/en-us/dotnet/api/system.io.path.getdirectoryname?view=net-6.0
                        var newExtendedDirPath = Path.GetDirectoryName(extendedPath.TrimEnd('/'));
                        if (!Directory.Exists(newExtendedDirPath))
                            Directory.CreateDirectory(newExtendedDirPath);
                    }

                    if (File.Exists(path) && !File.Exists(extendedPath))
                    {
                        File.Move(path, extendedPath);
                        return true;
                    }
                    else if (File.Exists(extendedPath))
                    {
                        return true;
                    }
                    else if (Directory.Exists(path) && !Directory.Exists(extendedPath))
                    {
                        Directory.Move(path, extendedPath);
                        return true;
                    }
                    else if (Directory.Exists(extendedPath))
                    {
                        return true;
                    }
                    else
                    {
                        LogMessage("File or directory does not exist. Skipping '" + path + "'. Maybe you have already excluded a parent directory or it has been deleted?", LogLevel.Warning);
                    }
                }
                else
                {
                    if (File.Exists(extendedPath))
                    {
                        if (File.Exists(path))
                        {
                            LogMessage("The file '" + path + "' already exists. It will be replaced with the hidden file '" + extendedPath + "'.", LogLevel.Warning);
                            File.Delete(path);
                        }

                        File.Move(extendedPath, path);
                        return true;
                    }
                    else if (File.Exists(path))
                    {
                        return true;
                    }
                    else if (Directory.Exists(extendedPath))
                    {
                        // If the normal directory got recreated (it happens) and is empty then delete it before moving stuff back
                        if (Directory.Exists(path))
                        {
                            if (Directory.GetFiles(path).Length == 0)
                            {
                                LogMessage("The directory '" + path + "' already exists but is empty. It will be replaced with the hidden directory '" + extendedPath + "'.", LogLevel.Warning);
                                Directory.Delete(path, true);
                            }
                            else
                            {
                                // ask if conflict dialog
                                bool replace = false;
                                var settings = ExcludeFromBuildSettings.GetOrCreateSettings();
                                if (settings.AskIfConflict)
                                {
                                    replace = EditorUtility.DisplayDialog(
                                        "Conflicting files/dirs",
                                        "The directory '" + path + "' already exists in the poject. It probably was recreated automatically by some code.\n\nDo you want to replace it with the hidden directory '" + extendedPath + "'?\n\nHint: you can disable this dialog in the settings ('AskIfConflict').",
                                        "Yes (delete and replace)", "No (abort revert)"
                                    );
                                }
                                else
                                {
                                    replace = true;
                                }

                                if (replace)
                                {
                                    LogMessage("The directory '" + path + "' already exists in the poject. Maybe it was recreated by some code? It will deleted and replaced with the hidden directory '" + extendedPath + "'.", LogLevel.Warning);
                                    Directory.Delete(path, true);
                                }
                                else
                                {
                                    LogMessage("The directory '" + path + "' already exists in the poject. User denied replacement. Will show this error for restoring '" + extendedPath + "'.", LogLevel.Error);
                                    return false;
                                }
                            }
                        }

                        if (!Directory.Exists(path))
                            Directory.Move(extendedPath, path);
                        else
                            return false;

                        return true;
                    }
                    else if (Directory.Exists(path))
                    {
                        return true;
                    }
                }
            }
            catch (System.Exception e)
            {
                LogMessage("An error occurred while renaming files or folders: " + e.Message + " Path: '" + path + "'", LogLevel.Error);

                bool isAccessDenied = e.Message.Contains("ccess") && e.Message.Contains("denied");
                if (isAccessDenied)
                {
                    string msg = "Maybe your IDE is locking some files. Please try to close it or make it unlock the files you are excluding.";
                    LogMessage(msg, LogLevel.Error);
                    // EditorUtility.DisplayDialog("Exclude from Build: Access denied error!", msg, "Ok");
                }

                return false;
            }

            return false;
        }

        async void waitForBuildCompletion(BuildReport report)
        {
            while (BuildPipeline.isBuildingPlayer || report == null || report.summary.result == BuildResult.Unknown)
            {
                await Task.Delay(1000);
            }

            OnPostprocessBuild(report);
        }

        public void OnPostprocessBuild(BuildReport report)
        {
            SessionState.EraseBool("DontDelayBuildAgain");

            // If TestAwareBuild is active and a test is running then don't execute the revert logic.
            var settings = ExcludeFromBuildSettings.GetOrCreateSettings();
            if (!(settings.TestAwareBuild && ExcludeFromBuildWindow.IsTesting))
                RevertExcludedFileAndDirNames();
        }

        public void OnProcessScene(Scene scene, BuildReport report)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            LogMessage("Evaluating scene '" + scene.name + "'", LogLevel.Log);

            var data = ExcludeFromBuildData.GetOrCreateData();

            var roots = scene.GetRootGameObjects();
            foreach (var root in roots)
            {
                var comps = root.GetComponentsInChildren<ExcludeFromBuildComponent>(includeInactive: true);
                foreach (var comp in comps)
                {
                    LogMessage("Executing scene '" + scene.name + "', component '" + comp.gameObject.name + "'", LogLevel.Log);
                    comp.Execute(data.CurrentGroup.Id);
                }
            }
        }

        public static bool ExcludedObjectsAreHidden()
        {
            var data = ExcludeFromBuildData.GetOrCreateData();
            if (data.HasExcludedObjects)
            {
                if (File.Exists(data.ExcludedObjects[0].AssetPath + "~") || Directory.Exists(data.ExcludedObjects[0].AssetPath + "~"))
                    return true;
            }
            return false;
        }
    }
}
