using UnityEditor;
using UnityEditor.Build;

namespace Kamgam.ExcludeFromBuild
{
    public class BuildTargetChangeDetector : IActiveBuildTargetChanged
    {
        public int callbackOrder => 100;

        public void OnActiveBuildTargetChanged(BuildTarget previousTarget, BuildTarget newTarget)
        {
            var data = ExcludeFromBuildData.GetOrCreateData();
            data.OnActiveBuildTargetChanged(previousTarget, newTarget);
        }
    }
}
