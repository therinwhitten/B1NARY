#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

namespace B1NARY.DLC
{
    public class ExcludeFromMainProject
    {
        [MenuItem("Assets/Exclude DLC Asset Bundles from Build")]
        public static void ExcludeDLCAssetBundlesFromBuild()
        {
            // Specify the folder where the asset bundles are located
            string assetBundleFolder = "Assets/AssetBundles";

            // Specify the folder where the DLC .txt files are located within the StreamingAssets folder
            string dlcDocsFolder = Path.Combine(Application.streamingAssetsPath, "Docs", "DLC");

            // Specify the folder where the voice files are located within the Resources folder
            string voiceFolder = "Assets/Resources/Voice/DLC";

            // Get all asset bundle paths in the asset bundle folder
            string[] assetBundlePaths = Directory.GetFiles(assetBundleFolder, "*.dlc");

            // Iterate through each asset bundle
            foreach (string assetBundlePath in assetBundlePaths)
            {
                string assetBundleName = Path.GetFileName(assetBundlePath);

                // Exclude asset bundle from the build
                File.Delete(assetBundlePath);
            }

            // Mark all .txt files in the DLC documents folder and its subfolders as Editor only
            MarkFilesAsEditorOnly(dlcDocsFolder, "*.txt");

            // Mark all voice files in the DLC voice folder and its subfolders as Editor only
            MarkFilesAsEditorOnly(voiceFolder, "*.*");

            Debug.Log("DLC asset bundles, associated .txt files, and voice files excluded from the build.");
        }

        private static void MarkFilesAsEditorOnly(string folderPath, string searchPattern)
        {
            // Get all files in the specified folder and its subfolders
            string[] files = Directory.GetFiles(folderPath, searchPattern, SearchOption.AllDirectories);

            // Iterate through each file
            foreach (string filePath in files)
            {
                // Mark the file as Editor only
                AssetImporter assetImporter = AssetImporter.GetAtPath(filePath);
                if (assetImporter != null)
                {
                    assetImporter.assetBundleName = null; // Clear asset bundle name
                    assetImporter.SetAssetBundleNameAndVariant(null, null);
                    assetImporter.SaveAndReimport();
                }
            }
        }
    }
}
#endif
