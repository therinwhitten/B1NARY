namespace B1NARY.DLC
{
    using System.Collections;
    using UnityEngine;
    using System.IO; // Add this namespace for Path.Combine

    public class BundleLoader : MonoBehaviour
    {
        public string bundleName;

        void Start()
        {
            StartCoroutine(LoadAssetBundle());
        }

        IEnumerator LoadAssetBundle()
        {
            // Construct the path to the asset bundle
            string path = Path.Combine(Application.streamingAssetsPath, "AssetBundles", bundleName);

            var request = AssetBundle.LoadFromFileAsync(path);
            yield return request;

            AssetBundle assetBundle = request.assetBundle;

            // Use the asset bundle as needed
            // For example, load a prefab
            var prefabRequest = assetBundle.LoadAssetAsync<GameObject>("MyPrefab");
            yield return prefabRequest;

            Instantiate(prefabRequest.asset);

            // Unload the asset bundle when done
            assetBundle.Unload(false);
        }
    }
}