using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;
using System.IO;

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
            Debug.Log("Start loading asset bundles...");

            yield return LoadAssetBundle(hentaiArtBundleName);
            yield return LoadAssetBundle(hentaiScenesBundleName);

            // Log that asset bundles are loaded
            Debug.Log("Asset Bundles are loaded.");
        }

        IEnumerator LoadAssetBundle(string bundleName)
        {
            // Construct the path to the asset bundle
            string path = Path.Combine(Application.streamingAssetsPath, "HentaiDLC", bundleName);
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
                StartCoroutine(LoadScenesFromAssetBundle(assetBundle));
            }

            // Unload the asset bundle when done
            //assetBundle.Unload(false);
            //Debug.Log("Asset Bundle unloaded: " + bundleName);
        }

        IEnumerator LoadScenesFromAssetBundle(AssetBundle assetBundle)
        {
            // Replace with the actual scene names in your asset bundle
            string sceneName1 = "StarBedroomFemaleHScene";
            string sceneName2 = "StarBedroomMaleHScene";

#if UNITY_EDITOR
            // Check if the scenes are in the build settings
            if (SceneInBuildSettings(sceneName1) && SceneInBuildSettings(sceneName2))
#endif
            {
                // Load the first scene asynchronously
                AsyncOperation asyncOperation1 = UnitySceneManager.LoadSceneAsync(sceneName1, LoadSceneMode.Additive);
                asyncOperation1.allowSceneActivation = false;

                // Load the second scene asynchronously
                AsyncOperation asyncOperation2 = UnitySceneManager.LoadSceneAsync(sceneName2, LoadSceneMode.Additive);
                asyncOperation2.allowSceneActivation = false;

                // Wait for both scenes to finish loading
                while (!asyncOperation1.isDone || !asyncOperation2.isDone)
                {
                    yield return null;
                }

                // Activate the scenes
                Scene scene1 = UnitySceneManager.GetSceneByName(sceneName1);
                Scene scene2 = UnitySceneManager.GetSceneByName(sceneName2);
                UnitySceneManager.SetActiveScene(scene1);
                UnitySceneManager.SetActiveScene(scene2);

                Debug.Log("Scenes loaded from the Asset Bundle.");
            }
#if UNITY_EDITOR
            else
            {
                Debug.LogError("One or more scenes are not in the build settings.");
            }
#endif
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
