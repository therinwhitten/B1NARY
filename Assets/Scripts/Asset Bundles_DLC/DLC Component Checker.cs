using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace B1NARY.DLC
{
    public class DLCComponentChecker : MonoBehaviour
    {
        public string hentaiArtBundleName = "hentaiart.dlc";
        public string hentaiScenesBundleName = "hentaiscenes.dlc";

        public List<GameObject> activateWhenLoaded; // List of GameObjects to activate when bundles are loaded
        public List<GameObject> deactivateWhenLoaded; // List of GameObjects to deactivate when bundles are loaded

        void Start()
        {
            StartCoroutine(CheckAssetBundlesWithDelay());
        }

        IEnumerator CheckAssetBundlesWithDelay()
        {
            Debug.Log("Checking asset bundles...");

            // Wait for a short delay before checking AssetBundles
            yield return new WaitForSeconds(1.0f); // Adjust the delay time as needed

            // Check if hentaiart.dlc is installed
            bool isHentaiArtInstalled = IsAssetBundleInstalled(hentaiArtBundleName);
            Debug.Log($"Hentai Art Bundle Installed: {isHentaiArtInstalled}");

            // Check if hentaiscenes.dlc is installed
            bool isHentaiScenesInstalled = IsAssetBundleInstalled(hentaiScenesBundleName);
            Debug.Log($"Hentai Scenes Bundle Installed: {isHentaiScenesInstalled}");

            // If either of the asset bundles is installed, enable the objects and disable others
            if (isHentaiArtInstalled || isHentaiScenesInstalled)
            {
                SetObjectsState(activateWhenLoaded, true);
                SetObjectsState(deactivateWhenLoaded, false);
            }
            else
            {
                // Both asset bundles are not installed, disable the objects and enable others
                SetObjectsState(activateWhenLoaded, false);
                SetObjectsState(deactivateWhenLoaded, true);
            }
        }

        void SetObjectsState(List<GameObject> objects, bool state)
        {
            // Iterate through the list and set the state of each object
            foreach (var obj in objects)
            {
                if (obj != null)
                {
                    obj.SetActive(state);
                }
            }
        }

        bool IsAssetBundleInstalled(string bundleName)
        {
            // Construct the path to the asset bundle
            string path = System.IO.Path.Combine(Application.streamingAssetsPath, "HentaiDLC", bundleName);

            // Check if the asset bundle file exists
            return System.IO.File.Exists(path);
        }
    }
}
