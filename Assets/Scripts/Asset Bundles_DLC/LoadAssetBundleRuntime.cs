namespace B1NARY.DLC
{
    using System.Collections;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using System.IO;

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
            Debug.Log("Start loading asset bundles...");

            yield return LoadAssetBundle(hentaiArtBundleName);
            yield return LoadAssetBundle(hentaiScenesBundleName);

            // Log that asset bundles are loaded
            Debug.Log("Asset Bundles are loaded.");
        }

        IEnumerator LoadAssetBundle(string bundleName)
        {
            // Construct the path to the asset bundle
            string path = Path.Combine(Application.streamingAssetsPath, "AssetBundles", bundleName);
            Debug.Log("Asset Bundle Path: " + path);

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

            if (bundleName.Equals(hentaiScenesBundleName))
            {
                // If it's the scenes bundle, load the scenes
                LoadScenesFromAssetBundle(assetBundle);
            }

            // Unload the asset bundle when done
            assetBundle.Unload(false);
            Debug.Log("Asset Bundle unloaded: " + bundleName);
        }

        void LoadScenesFromAssetBundle(AssetBundle assetBundle)
        {
            // Replace with the actual scene names in your asset bundle
            string sceneName1 = "StarBedroomFemaleHScene";
            string sceneName2 = "StarBedroomMaleHScene";

            // Load scenes from the asset bundle
            SceneManager.LoadScene(sceneName1, LoadSceneMode.Additive);
            SceneManager.LoadScene(sceneName2, LoadSceneMode.Additive);

            Debug.Log("Scenes loaded from the Asset Bundle.");
        }
    }
}
