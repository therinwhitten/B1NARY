#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
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

            // Get the names of documents in hentaiart.dlc
            List<string> documentNames = GetDocumentNamesFromAssetBundle("hentai.dlc");

            // Exclude the hentaiscenes.dlc and hentaiart.dlc asset bundles from the build
            ExcludeAssetBundlesAndDocuments(outputFolder, new string[] { "hentaiscenes.dlc", "hentaiart.dlc" }, documentNames);

            // Log whether or not the asset bundles are in the game
            Debug.Log("Asset Bundles are " + (AssetBundlesExistInGame(outputFolder)? "in the game" : "not in the game"));
        }

        private static void BuildAssetBundles(string outputPath, BuildAssetBundleOptions options, BuildTarget target)
        {
            // Build asset bundles for the current build target
            BuildPipeline.BuildAssetBundles(outputPath, options, target);
        }

        private static List<string> GetDocumentNamesFromAssetBundle(string assetBundleName)
        {
            List<string> documentNames = new List<string>();

            // Load the asset bundle
            AssetBundle assetBundle = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, assetBundleName));

            if (assetBundle != null)
            {
                // Get all asset paths in the asset bundle
                string[] assetPaths = assetBundle.GetAllAssetNames();

                // Filter out document names
                foreach (string assetPath in assetPaths)
                {
                    // Check if the asset path represents a document
                    if (IsDocument(assetPath))
                    {
                        // Get the document name and add it to the list
                        string documentName = Path.GetFileName(assetPath);
                        documentNames.Add(documentName);
                    }
                }

                // Unload the asset bundle
                assetBundle.Unload(false);
            }

            return documentNames;
        }

        private static bool IsDocument(string assetPath)
        {
            // Customize this method to determine if the asset path represents a document
            // For example, you can check if the asset path ends with ".txt" or any other criterion
            return assetPath.EndsWith(".txt");
        }

        private static void ExcludeAssetBundlesAndDocuments(string outputPath, string[] assetBundleNames, List<string> documentNames)
        {
            // Exclude the specified asset bundles from the build
            foreach (string assetBundleName in assetBundleNames)
            {
                string assetBundlePath = Path.Combine(outputPath, assetBundleName);
                File.Delete(assetBundlePath);
            }

            // Exclude the specified documents from the build
            foreach (string documentName in documentNames)
            {
                string documentPath = Path.Combine(outputPath, documentName);
                File.Delete(documentPath);
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
