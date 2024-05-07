using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace Kamgam.ExcludeFromBuild
{
    public class ExcludeFromBuildData : ScriptableObject, ISerializationCallbackReceiver
    {
        public const string DataFilePath = "Assets/ExcludeFromBuildData.asset";

        /// <summary>
        /// Mirror of StandaloneBuildSubtarget on old Unity versions since this has only been added in Unity 2021.2.
        /// SubTargets can be set in code like this:
        /// var buildPlayerOptions = new BuildPlayerOptions()
        /// {
        ///     subtarget = (int) StandaloneBuildSubtarget.Server,
        ///     target = BuildTarget.StandaloneWindows64,
        ///     options = BuildOptions.Development
        // };
        /// </summary>
        public enum StandaloneBuildSubtarget
        {
            Any,
            Server,
            Player
        }

        public enum BuildConfiguration
        {
            Any,
            Debug,
            Release
        }

        [System.Serializable]
        public class Group
        {
            public const string STANDALONE_SUB_TARGET_SERVER = "EFB_STANDALONE_SUB_TARGET_SERVER";

            public int Id;
            public string Name;
            public List<BuildTarget> BuildTargets = new List<BuildTarget>();
            public List<ExcludedObject> ExcludedObjects;
            public BuildConfiguration BuildConfiguration = BuildConfiguration.Any;

            /// <summary>
            /// The sub target which adds addition to BuildTargets.
            /// </summary>
            public StandaloneBuildSubtarget StandaloneSubTarget = StandaloneBuildSubtarget.Player;

            /// <summary>
            /// A comma separated list of defines which act as a filter in addition to BuildTargets.<br />
            /// Defines are combined with AND logic. All specified defines need to exists for this to be a positive match.
            /// </summary>
            public string Defines = "";

            public Group(int id, string name, params BuildTarget[] buildTargets)
            {
                Id = id;
                Name = name;
                BuildTargets = new List<BuildTarget>();
                foreach (var target in buildTargets)
                {
                    BuildTargets.Add(target);
                }
                StandaloneSubTarget = StandaloneBuildSubtarget.Player;
                ExcludedObjects = new List<ExcludedObject>();
                Defines = "";
            }

            public Group(int id, Group objectToCopy)
            {
                Id = id;
                Name = objectToCopy.Name;
                BuildTargets = new List<BuildTarget>();
                if (objectToCopy.BuildTargets != null)
                {
                    foreach (var target in objectToCopy.BuildTargets)
                    {
                        BuildTargets.Add(target);
                    }
                }
                ExcludedObjects = new List<ExcludedObject>();
                if (objectToCopy.ExcludedObjects != null)
                {
                    foreach (var obj in objectToCopy.ExcludedObjects)
                    {
                        var newObj = new ExcludedObject(obj);
                        ExcludedObjects.Add(newObj);
                    }
                }
                StandaloneSubTarget = objectToCopy.StandaloneSubTarget;
                Defines = objectToCopy.Defines;
            }

            public float GetSpecificityRating()
            {
                float specificity = 0;

                // The fewer build targets the higher the specificity (normalized 0.0 to 1.0)
                specificity += 1f - (BuildTargets.Count / 100f);

                if (string.IsNullOrEmpty(Defines))
                    specificity++;

                if (StandaloneSubTarget != StandaloneBuildSubtarget.Any)
                    specificity++;

                if (BuildConfiguration != BuildConfiguration.Any)
                    specificity++;

                return specificity;
            }

            /// <summary>
            /// For Unity 2021.2+ the EditorUserBuildSettings.standaloneBuildSubtarget is checked.<br />
            /// If the STANDALONE_SUB_TARGET_SERVER define is set ("EFB_STANDALONE_SUB_TARGET_SERVER") then sub target "Server" is assumed.<br />
            /// This was added to support Unity below 2021.2 too.
            /// </summary>
            /// <returns></returns>
            public bool MatchesStandaloneSubTarget()
            {
                if (StandaloneSubTarget == StandaloneBuildSubtarget.Any)
                    return true;

                string currentDefineSymbols = getCurrentBuildTargetDefinesString();

                bool currentBuildTargetIsServer = containsDefine(currentDefineSymbols, STANDALONE_SUB_TARGET_SERVER);
                if (StandaloneSubTarget == StandaloneBuildSubtarget.Server && currentBuildTargetIsServer)
                    return true;

                if (StandaloneSubTarget == StandaloneBuildSubtarget.Player && currentBuildTargetIsServer)
                    return false;

                // If it is not a standalone build then always assume sub target is player.
                // We need to do this because EditorUserBuildSettings.standaloneBuildSubtarget does only
                // reset to "Player" if switched to a build target that is not standalone. This means if
                // a user switches from "Dedicated Server" to "Android" then EditorUserBuildSettings.standaloneBuildSubtarget
                // will still be on the "Server" setting despite the build target being a mobile build.
                if (!IsStandaloneBuildTargetActive())
                {
                    if (StandaloneSubTarget == StandaloneBuildSubtarget.Server)
                        return false;

                    if (StandaloneSubTarget == StandaloneBuildSubtarget.Player)
                        return true;
                }

                switch (StandaloneSubTarget)
                {
                    case StandaloneBuildSubtarget.Server:
#if UNITY_2021_2_OR_NEWER
                        return EditorUserBuildSettings.standaloneBuildSubtarget == UnityEditor.StandaloneBuildSubtarget.Server;
#else
                        return currentBuildTargetIsServer;
#endif

                    case StandaloneBuildSubtarget.Player:
#if UNITY_2021_2_OR_NEWER
                        return EditorUserBuildSettings.standaloneBuildSubtarget == UnityEditor.StandaloneBuildSubtarget.Player;
#else
                        return !currentBuildTargetIsServer;
#endif

                    case StandaloneBuildSubtarget.Any:
                    default:
                        return true;
                }
            }

            private static bool IsStandaloneBuildTargetActive()
            {
#pragma warning disable CS0618 // Type or member is obsolete
                return EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneLinux
                    || EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneLinux64
                    || EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneLinuxUniversal
                    || EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneOSX
                    || EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneOSXIntel
                    || EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneOSXIntel64
                    || EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneWindows
                    || EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneWindows64;
#pragma warning restore CS0618 // Type or member is obsolete
            }

            public bool MatchesAllCtriteria(BuildTarget target)
            {
                return MatchesBuildTarget(target) && MatchesDefines() && MatchesStandaloneSubTarget() && MatchesBuildConfiguration();
            }

            public bool MatchesBuildTarget(BuildTarget target)
            {
                return BuildTargets.Contains(target);
            }

            public bool MatchesBuildConfiguration()
            {
                if (BuildConfiguration == BuildConfiguration.Any)
                    return true;

                // Thanks to https://forum.unity.com/threads/is-there-anyway-to-get-whether-development-build-by-editor-script.1176197/#post-7533251
                if (EditorUserBuildSettings.development)
                {
                    return BuildConfiguration == BuildConfiguration.Debug;
                }
                else
                {
                    return BuildConfiguration == BuildConfiguration.Release;
                }
            }

            /// <summary>
            /// Defines are combined with AND logic. All specified defines need to exists for this to be a positive match.
            /// </summary>
            /// <returns></returns>
            public bool MatchesDefines()
            {
                if (Defines == null)
                    Defines = "";
                var defines = Defines.Trim().Split(',');
                if (defines.Length == 0)
                    return true;

                string currentDefineSymbols = getCurrentBuildTargetDefinesString();
                bool allDefinesFound = true;
                foreach (var define in defines)
                {
                    if (string.IsNullOrEmpty(define))
                        continue;

                    if (!containsDefine(currentDefineSymbols, define))
                    {
                        allDefinesFound = false;
                    }
                }

                return allDefinesFound;
            }

            protected bool containsDefine(string definesString, string defineName)
            {
                return definesString == defineName || definesString.Contains("," + defineName) || definesString.Contains(defineName + ",");
            }

            protected string getCurrentBuildTargetDefinesString()
            {
                var group = EditorUserBuildSettings.selectedBuildTargetGroup;

#if UNITY_2023_1_OR_NEWER
                string currentDefineSymbols = PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(group));
#else
                string currentDefineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
#endif
                return currentDefineSymbols.Replace(" ", "");
            }
        }

        public List<Group> Groups = new List<Group>();
        public bool HasGroups => Groups != null && Groups.Count > 0;

        [SerializeField]
        protected int lastGroupId = 0;
        public int GetNextGroupId()
        {
            if (!HasGroups)
                return 0;
            else
            {
                bool taken;
                int newId;
                do
                {
                    taken = false;
                    newId = lastGroupId++ % 10000000;
                    foreach (var platform in Groups)
                    {
                        if (platform.Id == newId)
                        {
                            taken = true;
                            break;
                        }
                    }
                }
                while (taken);
                return newId;
            }
        }

        [SerializeField]
        protected int currentGroupId = -1;

        public Group CurrentGroup
        {
            get
            {
                // ensure there is at least one group.
                if (Groups == null)
                    Groups = new List<Group>();
                if (Groups.Count == 0)
                {
                    // add default group with all build targets
                    var group = new Group(GetNextGroupId(), "Default");
                    var type = typeof(BuildTarget);
                    foreach (int value in System.Enum.GetValues(type))
                        group.BuildTargets.Add((BuildTarget)value);
                    Groups.Add(group);

                    currentGroupId = group.Id;
                }

                // If no group is active then try to find one OR if none is found then add the current target to the first group.
                if (currentGroupId < 0)
                {
                    // get the first one matching the current build target
                    var group = GetFirstGroupMatchingTarget(EditorUserBuildSettings.activeBuildTarget);
                    if (group == null)
                    {
                        // Use first group and add target to it.
                        group = Groups[0];
                        group.BuildTargets.Add(EditorUserBuildSettings.activeBuildTarget);
                        EditorUtility.SetDirty(this);
                        AssetDatabase.SaveAssets();
                    }
                    currentGroupId = group.Id;
                }

                // find current group by id
                for (int i = 0; i < Groups.Count; i++)
                {
                    if (Groups[i].Id == currentGroupId)
                    {
                        return Groups[i];
                    }
                }

                return null;
            }

            set
            {
                if (value != null)
                {
                    currentGroupId = value.Id;
                    SessionState.SetInt(ExcludeFromBuildComponent.SessionActiveGroupIdKey, currentGroupId);
                }
            }
        }

        public void SetCurrentGroupById(int groupId)
        {
            CurrentGroup = GetGroupById(groupId);
        }

        public List<ExcludedObject> ExcludedObjects
        {
            get
            {
                return CurrentGroup.ExcludedObjects;
            }
        }
        public bool HasExcludedObjects => ExcludedObjects != null && ExcludedObjects.Count > 0;

        /// <summary>
        /// Sort by path to ensure parent directories are handled BEFORE their children.
        /// </summary>
        public void SortExcludedObjects()
        {
            if (ExcludedObjects == null || ExcludedObjects.Count == 0)
                return;

            ExcludedObjects.Sort(sortCompareFunc);
        }

        private int sortCompareFunc(ExcludedObject a, ExcludedObject b)
        {
            return string.Compare(a.Path, b.Path, true, System.Globalization.CultureInfo.InvariantCulture);
        }

        public void OnActiveBuildTargetChanged(BuildTarget previousTarget, BuildTarget newTarget)
        {
            CurrentGroup = GetFirstGroupMatchingTarget(newTarget);
        }

        public static ExcludeFromBuildData GetOrCreateData()
        {
            var data = AssetDatabase.LoadAssetAtPath<ExcludeFromBuildData>(DataFilePath);
            if (data == null)
            {
                data = ScriptableObject.CreateInstance<ExcludeFromBuildData>();
                AssetDatabase.CreateAsset(data, DataFilePath);
                AssetDatabase.SaveAssets();
            }
            return data;
        }

        internal static SerializedObject GetSerializedData()
        {
            return new SerializedObject(GetOrCreateData());
        }

        public static void SelectData()
        {
            var settings = ExcludeFromBuildSettings.GetOrCreateSettings();
            if (settings != null)
            {
                Selection.activeObject = settings;
                EditorGUIUtility.PingObject(settings);
            }
            else
            {
                EditorUtility.DisplayDialog("Error", "ExcludeFromBuild data could not be found or created.", "Ok");
            }
        }

        static bool _ignoreActiveGroup = false;

        public static void IgnoreActiveGroup()
        {
            _ignoreActiveGroup = true;
        }

        public static void UnignoreActiveGroup()
        {
            _ignoreActiveGroup = false;
        }

        public Group GetFirstGroupMatchingTarget(BuildTarget target)
        {
            if (!HasGroups)
                return null;

            // First: check if the current active group matches
            if (!_ignoreActiveGroup)
            {
                for (int i = 0; i < Groups.Count; i++)
                {
                    if (Groups[i].Id != currentGroupId)
                        continue;

                    if (Groups[i].MatchesAllCtriteria(target))
                    {
                        return Groups[i];
                    }
                }
            }

            // Second: Search all except "Default".
            float specificity = -1;
            Group specificGroup = null;
            for (int i = 0; i < Groups.Count; i++)
            {
                if (Groups[i].Name.ToLower() == "default")
                    continue;

                if (Groups[i].MatchesAllCtriteria(target))
                {
                    if (Groups[i].GetSpecificityRating() > specificity)
                    {
                        specificity = Groups[i].GetSpecificityRating();
                        specificGroup = Groups[i];
                    }
                }
            }

            if (specificGroup != null)
                return specificGroup;

            // Third: Search all groups
            specificity = -1;
            specificGroup = null;
            for (int i = 0; i < Groups.Count; i++)
            {
                if (Groups[i].MatchesAllCtriteria(target))
                {
                    if (Groups[i].GetSpecificityRating() > specificity)
                    {
                        specificity = Groups[i].GetSpecificityRating();
                        specificGroup = Groups[i];
                    }
                }
            }

            if (specificGroup != null)
                return specificGroup;

            return null;
        }

        public Group GetGroupById(int groupId)
        {
            if (!HasGroups)
                return null;

            foreach (var group in Groups)
            {
                if (group.Id == groupId)
                    return group;
            }

            return null;
        }

        public void Clear()
        {
            if (ExcludedObjects != null)
                ExcludedObjects.Clear();
        }

        public ExcludedObject GetExcludedObject(string guid)
        {
            if (ExcludedObjects == null || ExcludedObjects.Count == 0)
                return null;

            foreach (var obj in ExcludedObjects)
                if (obj.AssetGUID == guid)
                    return obj;

            return null;
        }

        public bool IsExcluded(string guid)
        {
            if (ExcludedObjects == null || ExcludedObjects.Count == 0)
                return false;

            return Contains(guid);
        }

        public bool IsExcludedInAnyGroup(string guid)
        {
            if (Groups == null || Groups.Count == 0)
                return false;

            foreach (var group in Groups)
            {
                if (group.ExcludedObjects == null || group.ExcludedObjects.Count == 0)
                    continue;

                foreach (var obj in group.ExcludedObjects)
                {
                    if (obj.AssetGUID == guid)
                        return true;
                }
            }

            return false;
        }

        public void Exclude(string guid, string path)
        {
            AddIfNotContained(guid, path);
        }

        public void Exclude(ExcludedObject obj)
        {
            AddIfNotContained(obj);
        }

        public void Include(string guid)
        {
            Remove(guid);
        }

        public void Include(ExcludedObject obj)
        {
            Remove(obj);
        }

        public bool Contains(string guid)
        {
            if (ExcludedObjects == null || ExcludedObjects.Count == 0)
                return false;

            foreach (var obj in ExcludedObjects)
                if (obj.AssetGUID == guid)
                    return true;

            return false;
        }

        public bool Contains(ExcludedObject obj)
        {
            if (ExcludedObjects == null || ExcludedObjects.Count == 0)
                return false;

            return ExcludedObjects.Contains(obj);
        }

        public void Remove(string guid)
        {
            if (ExcludedObjects == null || ExcludedObjects.Count == 0)
                return;

            for (int i = ExcludedObjects.Count - 1; i >= 0; i--)
                if (ExcludedObjects[i].AssetGUID == guid)
                    ExcludedObjects.RemoveAt(i);
        }

        public void Remove(ExcludedObject obj)
        {
            if (ExcludedObjects == null || ExcludedObjects.Count == 0)
                return;

            if (Contains(obj))
                ExcludedObjects.Remove(obj);
        }

        public ExcludedObject AddIfNotContained(string guid, string path)
        {
            if (ExcludedObjects == null)
                return null;

            if (!Contains(guid))
            {
                var excludedObject = new ExcludedObject(guid, path);
                ExcludedObjects.Add(excludedObject);
                SortExcludedObjects();
                return excludedObject;
            }

            return null;
        }

        public ExcludedObject AddIfNotContained(ExcludedObject obj)
        {
            if (ExcludedObjects == null)
                return null;

            if (!ExcludedObjects.Contains(obj))
            {
                ExcludedObjects.Add(obj);
                SortExcludedObjects();
                return obj;
            }

            return null;
        }

        public bool HasStreamingAssets()
        {
            foreach (var obj in ExcludedObjects)
            {
                if (!string.IsNullOrEmpty(obj.AssetPath) && obj.AssetPath.Contains("StreamingAssets"))
                    return true;
            }

            return false;
        }

        public void Save()
        {
            EditorUtility.SetDirty(this);

#if UNITY_2020_3_OR_NEWER
            AssetDatabase.SaveAssetIfDirty(this);
#else
            AssetDatabase.SaveAssets();
#endif
        }

        public void OnBeforeSerialize() { }

        public void OnAfterDeserialize()
        {
            EditorApplication.update += checkAfterDeserialization;
        }

        void checkAfterDeserialization()
        {
            int lastSetGroupId = SessionState.GetInt(ExcludeFromBuildComponent.SessionActiveGroupIdKey, -1);
            if (lastSetGroupId >= 0)
            {
                if (currentGroupId != lastSetGroupId)
                    ExcludeFromBuildController.LogMessage("Restoring last set group id to " + lastSetGroupId, LogLevel.Log);
                currentGroupId = lastSetGroupId;
            }

            EditorApplication.update -= checkAfterDeserialization;
        }

        public int GetGroupIndex(Group group)
        {
            if (group == null)
                return -1;

            for (int i = 0; i < Groups.Count; i++)
            {
                if (Groups[i] == group)
                    return i;
            }

            return -1;
        }

        // Static methods for batch build ative group selection

        public static void SetActiveGroupByIndex(int index)
        {
            var data = GetOrCreateData();
            if (data != null)
            {
                if (index >= 0 && index < data.Groups.Count)
                {
                    var group = data.Groups[index];
                    data.CurrentGroup = group;

                    string msg = "ExcludeFromBuild: Set active group to '" + group.Name + "' (index: " + index + ").";
                    Debug.Log(msg);
                    System.Console.WriteLine("LOG: " + msg);

                }
                else
                {
                    string msg = "ExcludeFromBuild: No group with index " + index;
                    Debug.LogWarning(msg);
                    System.Console.WriteLine("WARNING: " + msg);
                }
            }
        }

        public static void TestBatchLog()
        {
            string msg = "Test batch log message";
            Debug.LogWarning(msg);
            System.Console.WriteLine("LOG: " + msg);
        }

        public static void SetActiveGroup0() => SetActiveGroupByIndex(0);
        public static void SetActiveGroup1() => SetActiveGroupByIndex(1);
        public static void SetActiveGroup2() => SetActiveGroupByIndex(2);
        public static void SetActiveGroup3() => SetActiveGroupByIndex(3);
        public static void SetActiveGroup4() => SetActiveGroupByIndex(4);
        public static void SetActiveGroup5() => SetActiveGroupByIndex(5);
        public static void SetActiveGroup6() => SetActiveGroupByIndex(6);
        public static void SetActiveGroup7() => SetActiveGroupByIndex(7);
        public static void SetActiveGroup8() => SetActiveGroupByIndex(8);
        public static void SetActiveGroup9() => SetActiveGroupByIndex(9);
    }
}