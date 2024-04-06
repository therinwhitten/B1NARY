using UnityEngine;

namespace Kamgam.ExcludeFromBuild.Examples
{
    public class EFBTranslator : MonoBehaviour
    {
        protected float v;

        void Update()
        {
            v += 0.02f;
            var pos = transform.localPosition;
            pos.y = Mathf.Sin(v) * 3f;
            transform.localPosition = pos;
        }
    }
}
