#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Linq;
using System.IO;

namespace B1NARY.DLC
{
    public class ExcludeFromMainProject
    {
        [MenuItem("Assets/Build AssetBundles")]
        public static void BuildAllAssetBundles()
        {
            // Get the current build target
            BuildTarget activeBuildTarget = EditorUserBuildSettings.activeBuildTarget;

            // Specify the folder where the asset bundles should be built
            string outputFolder = "Assets/AssetBundles";

            // Build asset bundles for the current build target
            BuildAssetBundles(outputFolder, BuildAssetBundleOptions.None, activeBuildTarget);

            // Exclude the hentaiscenes.dlc and hentaiart.dlc asset bundles from the build
            ExcludeAssetBundles(outputFolder, new string[] { "hentaiscenes.dlc", "hentaiart.dlc" });

            // Log whether or not the asset bundles are in the game
            Debug.Log("Asset Bundles are " + (AssetBundlesExistInGame(outputFolder)? "in the game" : "not in the game"));
        }

        private static void BuildAssetBundles(string outputPath, BuildAssetBundleOptions options, BuildTarget target)
        {
            // Build asset bundles for the current build target
            BuildPipeline.BuildAssetBundles(outputPath, options, target);
        }

        private static void ExcludeAssetBundles(string outputPath, string[] assetBundleNames)
        {
            // Exclude the specified asset bundles from the build
            foreach (string assetBundleName in assetBundleNames)
            {
                string assetBundlePath = Path.Combine(outputPath, assetBundleName);
                File.Delete(assetBundlePath);
            }
        }

        private static bool AssetBundlesExistInGame(string outputPath)
        {
            // Check if asset bundles exist in the game by looking for scenes from the asset bundles
            string sceneNameMale = "Star Bedroom Male H Scene"; // Change this to the scene name from the male asset bundle
            string sceneNameFemale = "Star Bedroom Female H Scene"; // Change this to the scene name from the female asset bundle

            string scenePathMale = Path.Combine(outputPath, sceneNameMale + ".unity");
            string scenePathFemale = Path.Combine(outputPath, sceneNameFemale + ".unity");

            return File.Exists(scenePathMale) && File.Exists(scenePathFemale);
        }

    }
}
#endif