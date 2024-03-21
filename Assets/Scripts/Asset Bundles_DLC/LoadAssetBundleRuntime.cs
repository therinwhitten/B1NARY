using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System.Linq;
using B1NARY;
using UnityEngine.Diagnostics;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;

namespace B1NARY.DLC
{
    public class LoadAssetBundleRuntime : MonoBehaviour
    {
        public string hentaiArtBundleName = "hentaiart.dlc";
        public string hentaiScenesBundleName = "hentaiscenes.dlc";

        void Start()
        {
            StartCoroutine(LoadAssetBundles());
        }

        IEnumerator LoadAssetBundles()
        {
            // Check if there are asset bundles to load
            if (!AssetBundleExists(hentaiArtBundleName) && !AssetBundleExists(hentaiScenesBundleName))
            {
                yield break;
            }

            if (AssetBundleExists(hentaiArtBundleName) && !IsAssetBundleLoaded(hentaiArtBundleName))
            {
                yield return LoadAssetBundle(hentaiArtBundleName);
            }

            if (AssetBundleExists(hentaiScenesBundleName) && !IsAssetBundleLoaded(hentaiScenesBundleName))
            {
                yield return LoadAssetBundle(hentaiScenesBundleName);
            }
        }

        bool AssetBundleExists(string bundleName)
        {
            string path = Path.Combine(Application.streamingAssetsPath, "DLC", bundleName);
            return File.Exists(path);
        }

        bool IsAssetBundleLoaded(string bundleName)
        {
            return AssetBundle.GetAllLoadedAssetBundles().Any(bundle => bundle.name == bundleName);
        }

        IEnumerator LoadAssetBundle(string bundleName)
        {
            // Construct the path to the asset bundle
            string path = Path.Combine(Application.streamingAssetsPath, "DLC", bundleName);

            var request = AssetBundle.LoadFromFileAsync(path);
            yield return request;

            AssetBundle assetBundle = request.assetBundle;

            if (assetBundle == null)
            {
                Debug.LogError("Failed to load AssetBundle: " + path);
                yield break; // Exit the coroutine if the AssetBundle couldn't be loaded
            }

            // Use the asset bundle as needed
            Debug.Log("Asset Bundle loaded successfully: " + bundleName);
        }

#if UNITY_EDITOR
        // Check if the scene is in the build settings
        private bool SceneInBuildSettings(string sceneName)
        {
            foreach (var scene in UnityEditor.EditorBuildSettings.scenes)
            {
                if (scene.path.Contains(sceneName))
                {
                    return true;
                }
            }
            return false;
        }
#endif
    }
}
