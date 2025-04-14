using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;

namespace Kamgam.ExcludeFromBuild
{
    public static class PrefabPreProcessor
    {
        public const string BACKUP_EXTENSION = ".backup~";

        public static void LogMessage(string message, LogLevel logLevel = LogLevel.Log)
        {
            ExcludeFromBuildController.LogMessage(message, logLevel);
        }

        public static string LoadPaths()
        {
            var data = ExcludeFromBuildData.GetOrCreateData();
            return data.PreProcessedPrefabPaths;
        }

        public static void SavePaths(string paths)
        {
            var data = ExcludeFromBuildData.GetOrCreateData();
            data.PreProcessedPrefabPaths = paths;
            EditorUtility.SetDirty(data);
#if UNITY_2021_3_OR_NEWER
            AssetDatabase.SaveAssetIfDirty(data); 
#else
            AssetDatabase.SaveAssets();
#endif
        }

        public static void PreProcess()
        {
            string paths = "";
            var data = ExcludeFromBuildData.GetOrCreateData();

            var prefabGuids = AssetDatabase.FindAssets("t:Prefab");

            foreach (string guid in prefabGuids)
            {
                string prefabPath = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

                if (prefab != null)
                {
                    var excludeComps = prefab.GetComponentsInChildren<ExcludeFromBuildComponent>(includeInactive: true);
                    if (excludeComps.Length > 0)
                    {
                        // Make a hidden copy of the prefab file.
                        string backupPath = Path.Combine(Path.GetDirectoryName(prefabPath), Path.GetFileNameWithoutExtension(prefabPath) + ".prefab" + BACKUP_EXTENSION);

                        LogMessage("Prefab PreProcessing on " + prefabPath);

                        File.Copy(prefabPath, backupPath, overwrite: false);
                        paths += backupPath + ":";

                        // Execute exclusions
                        foreach (var comp in excludeComps)
                        {
                            comp.Execute(data.CurrentGroup.Id);
                        }
                    }
                }
                else
                {
                    Debug.LogWarning($"Prefab could not be loaded at path: {prefabPath}");
                }
            }

            SavePaths(paths);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static void PostProcess()
        {
            string pathsStr = LoadPaths();
            if (string.IsNullOrEmpty(pathsStr))
                return;
            
            var paths = pathsStr.Split(':');
            foreach (var backupPath in paths)
            {
                if (string.IsNullOrEmpty(backupPath))
                    continue;

                var path = backupPath.Replace(BACKUP_EXTENSION, "");

                if (File.Exists(backupPath) && File.Exists(path))
                {
                    File.Delete(path);
                    LogMessage("Prefab PostProcessing restoring " + path);
                    File.Move(backupPath, path);
                }
            }

            SavePaths("");
        }
    }
}