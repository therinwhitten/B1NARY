using UnityEngine;

namespace Kamgam.ExcludeFromBuild.Examples
{
    public class EFBRotator : MonoBehaviour
    {
        void Update()
        {
            this.transform.Rotate(Vector3.up, 1f);
        }
    }
}
