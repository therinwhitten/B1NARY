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

            // Log whether or not the asset bundles are in the game
            Debug.Log("Asset Bundles are " + (AssetBundlesExistInGame()? "in the game" : "not in the game"));
        }

        private static void BuildAssetBundles(string outputPath, BuildAssetBundleOptions options, BuildTarget target)
        {
            // Exclude the hentaiscenes.dlc and hentaiart.dlc asset bundles from the build
            string[] assetBundleNamesToExclude = { "hentaiscenes.dlc", "hentaiart.dlc" };

            AssetBundleBuild[] assetBundleBuilds = new AssetBundleBuild[1];
            assetBundleBuilds[0] = new AssetBundleBuild()
            {
                assetBundleName = "all_asset_bundles",
                assetNames = new string[] { }
            };

            foreach (string assetBundleName in assetBundleNamesToExclude)
            {
                string assetBundlePath = Path.Combine(outputPath, assetBundleName);
                assetBundleBuilds[0].assetNames = assetBundleBuilds[0].assetNames.Concat(AssetDatabase.GetAssetPathsFromAssetBundle(assetBundleName)).ToArray();
                AssetDatabase.RemoveAssetBundleName(assetBundleName, true);
            }

            BuildPipeline.BuildAssetBundles(outputPath, assetBundleBuilds, options, target);
        }

        private static bool AssetBundlesExistInGame()
        {
            // Check if asset bundles exist in the game by looking for a scene from the asset bundles
            string sceneNameToCheck = "Star Bedroom Male H Scene"; // Change this to the scene name from the asset bundle
            return EditorBuildSettings.scenes.Any(scene => scene.path.EndsWith(sceneNameToCheck + ".unity"));
        }
    }
}
#endif