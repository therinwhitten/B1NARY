using UnityEditor;

namespace Kamgam.ExcludeFromBuild
{
    [InitializeOnLoad]
    public static class CheckForHiddenObjectsAtStart
    {
        static CheckForHiddenObjectsAtStart()
        {
            EditorApplication.update += runOnce;
        }

        static void runOnce()
        {
            checkHiddenObjects();
            EditorApplication.update -= runOnce;
        }

        static void checkHiddenObjects()
        {
            if (SessionState.GetBool("Kamgam.ExcludeFromBuild.CheckForHiddenObjectsAtStart", false))
                return;

            if (ExcludeFromBuildController.ExcludedObjectsAreHidden())
            {
                if (EditorUtility.DisplayDialog("TEST in progress!", "ExcludeFromBuild has a test in progress. You should end the test before making any changes.", "OK (open Window)"))
                {
                    ExcludeFromBuildWindow.GetOrOpen();
                }
            }

            SessionState.SetBool("Kamgam.ExcludeFromBuild.CheckForHiddenObjectsAtStart", true);
        }
    }
}
