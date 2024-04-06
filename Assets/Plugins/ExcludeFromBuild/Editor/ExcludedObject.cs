using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static Kamgam.ExcludeFromBuild.ExcludeFromBuildData;

namespace Kamgam.ExcludeFromBuild
{
    [System.Serializable]
    public class ExcludedObject : ISerializationCallbackReceiver
    {
        public string ComponentSceneGUID;
        public string ComponentSceneName;
        public string ComponentGUID;

        /// <summary>
        /// The path within the scene at the time of creation.<br />
        /// This is not a reliable way of identification. Paths may not be outdated and are not guaranteed to be unique. Use the GUIDs to find the component in a scene.<br />
        /// Use this to display human readable text.
        /// </summary>
        public string ComponentPath;

        /// <summary>
        /// A list of all the groups this is active for (if it's an ExcludeFromBuild).
        /// This field is not serialized and is only valid after a scan.
        /// </summary>
        [System.NonSerialized]
        public List<int> ComponentGroupIds;

        /// <summary>
        /// Is active for all groups (no matter what ComponentGroupIds says).
        /// This field is not serialized and is only valid after a scan.
        /// </summary>
        [System.NonSerialized]
        public bool ComponentAllGroups;

        /// <summary>
        /// File or folder guid.
        /// </summary>
        public string AssetGUID;

        public bool IsAsset => AssetGUID != null;

        /// <summary>
        /// If it's an asset then this is the asset path relative to the project root (starts with "Assets/...)".<br />
        /// </summary>
        public string AssetPath;

        /// <summary>
        /// Returns either the AssetPath or the ComponentPath.
        /// </summary>
        public string Path => IsAsset ? AssetPath : ComponentPath;

        private bool miniThumbnailLoaded;
        protected Texture miniThumbnail;
        public Texture MiniThumbnail
        {
            get
            {
                if (!miniThumbnailLoaded)
                    loadThumbnail();
                return miniThumbnail;
            }

            set
            {
                miniThumbnail = value;
            }
        }

        public ExcludedObject(string sceneGUID, string sceneName, string componentGUID, string path, List<int> componentGroupIds, bool componentAllGroups)
        {
            ComponentSceneGUID = sceneGUID;
            ComponentSceneName = sceneName;
            ComponentGUID = componentGUID;
            ComponentPath = path;
            ComponentGroupIds = componentGroupIds;
            ComponentAllGroups = componentAllGroups;

            AssetGUID = null;
            AssetPath = null;

            loadThumbnail();
        }

        public ExcludedObject(string assetGUID, string path)
        {
            ComponentSceneGUID = null;
            ComponentGUID = null;
            ComponentPath = null;

            AssetGUID = assetGUID;
            AssetPath = path;

            loadThumbnail();
        }

        public ExcludedObject(ExcludedObject objectToCopy)
        {
            ComponentSceneGUID = objectToCopy.ComponentSceneGUID;
            ComponentSceneName = objectToCopy.ComponentSceneName;
            ComponentGUID = objectToCopy.ComponentGUID;
            ComponentPath = objectToCopy.ComponentPath;
            if (objectToCopy.ComponentGroupIds != null)
            {
                ComponentGroupIds = new List<int>();
                for (int i = 0; i < objectToCopy.ComponentGroupIds.Count; i++)
                {
                    ComponentGroupIds.Add(objectToCopy.ComponentGroupIds[i]);
                }
            }
            else
            {
                ComponentGroupIds = null;
            }
            ComponentAllGroups = objectToCopy.ComponentAllGroups;

            AssetGUID = objectToCopy.AssetGUID;
            AssetPath = objectToCopy.AssetPath;

            loadThumbnail();
        }

        public void OnBeforeSerialize() {}

        public void OnAfterDeserialize()
        {
            miniThumbnailLoaded = false;
        }

        protected void loadThumbnail()
        {
            if (ComponentGUID != null)
            { 
                MiniThumbnail = EditorGUIUtility.FindTexture("cs Script Icon");
            }
            else 
            {
                var asset = AssetDatabase.LoadAssetAtPath<Object>(AssetPath);
                if (asset != null)
                    MiniThumbnail = AssetPreview.GetMiniThumbnail(asset);
            }
        }
    }
}