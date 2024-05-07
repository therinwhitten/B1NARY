using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Kamgam.ExcludeFromBuild
{
    [AddComponentMenu("Exclude From Build")]
    public class ExcludeFromBuildComponent : MonoBehaviour
#if UNITY_EDITOR
        , ISerializationCallbackReceiver
#endif
    {
#if UNITY_EDITOR
        public string GUID;

        [System.NonSerialized]
        public const string SessionActiveGroupIdKey = "Kamgam.EFB.ActiveGroupId";

        [Tooltip("Exclude this whole GameObject?\nDisable to exclude only certain components.")]
        public bool GameObject = true;

        [Tooltip("Exclude these components from the build?")]
        public List<Component> Components;

        [Tooltip("Active for all groups?\nDisable to customize for which groups this component will be taken into account.")]
        public bool AllGroups = true;

        [Tooltip("The exclusions will only be executed if the currently active group is in the list.")]
        public List<int> GroupIds = new List<int>();

        [Tooltip("Simulate the removal in the PlayMode?")]
        public bool TestInPlayMode = false;

        public bool HasComponentsToExclude => Components != null && Components.Count > 0;

        public void Execute(int activeGroupId = -1)
        {
            // Abort and warn if activeGroup is unkown
            if (activeGroupId < 0)
            {
                Debug.LogWarning("ExcludeFromBuildComponent: Active group of '" + this.name + "' is unknown. Aborting.");
                return;
            }

            // skip if the group does not match
            if (!AllGroups && activeGroupId >= 0 && !GroupIds.Contains(activeGroupId))
                return;

            if (GameObject)
            {
                if(this.gameObject != null)
                    DestroyImmediate(this.gameObject);
            }
            else if (Components != null && Components.Count > 0)
            {
                foreach (var comp in Components)
                {
                    if (comp != null)
                        DestroyImmediate(comp);
                }
                DestroyImmediate(this);
            }
            else
            {
                DestroyImmediate(this);
            }
        }

        public string GetPath()
        {
            var path = new System.Text.StringBuilder();

            var parent = transform.parent;
            path.Append(gameObject.name);
            while (parent != null)
            {
                path.Insert(0, "/");
                path.Insert(0, parent.name);
                parent = parent.parent;
            }

            return path.ToString();
        }

        public void Awake()
        {
            createNewGUIDIfNeeded();

            if (EditorApplication.isPlaying && TestInPlayMode)
            {
                int activeGroupId = SessionState.GetInt(SessionActiveGroupIdKey, -1);
                Execute(activeGroupId);
            }
        }

        public void Reset()
        {
            createNewGUIDIfNeeded();
            AllGroups = true;
        }

        protected void createNewGUIDIfNeeded()
        {
            if (GUID == null)
            {
                GUID = System.Guid.NewGuid().ToString();
                EditorUtility.SetDirty(this);
            }
        }

        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize()
        {
            if (GUID == null)
            {
                GUID = System.Guid.NewGuid().ToString();
            }
        }
#endif
    }
}
