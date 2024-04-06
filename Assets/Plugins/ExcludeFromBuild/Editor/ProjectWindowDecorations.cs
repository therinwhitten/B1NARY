using UnityEditor;
using UnityEngine;

namespace Kamgam.ExcludeFromBuild
{
    [InitializeOnLoad]
    public static class ProjectWindowDecorations
    {
        static Texture texture;

        static ProjectWindowDecorations()
        {
            EditorApplication.projectWindowItemOnGUI += DrawAssetDetails;
            texture = DrawUtils.GetExcludeIconTexture();
        }

        private static void DrawAssetDetails(string guid, Rect rect)
        {
            if (Application.isPlaying || Event.current.type != EventType.Repaint || texture == null)
                return;

            var settings = ExcludeFromBuildSettings.GetOrCreateSettings();
            if (!settings.ShowIconInProjectView)
                return;

            var data = ExcludeFromBuildData.GetOrCreateData();
            if (data.IsExcluded(guid))
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                bool isFolder = AssetDatabase.IsValidFolder(assetPath);

#if UNITY_2020_1_OR_NEWER
                rect.x = rect.x + (isFolder ? 10 : 12);
                rect.y = rect.y + (isFolder ? 2 : 0);
                rect.width = Mathf.Min(10, rect.height * 0.33f);
                rect.height = Mathf.Min(10, rect.height * 0.33f);
#else
                rect.x = rect.x + (isFolder ? 6 : 8); 
                rect.y = rect.y - (isFolder ? 2 : 3);
                rect.width = 15;
                rect.height = 15;
#endif


                GUI.DrawTexture(rect, texture);
            }
        }
    }
}
