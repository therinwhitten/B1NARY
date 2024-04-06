using UnityEngine;

namespace Kamgam.ExcludeFromBuild.Examples
{
    public class EFBTranslatorExtended : EFBTranslator
    {
        void Update()
        {
            v += 0.06f;
            var pos = transform.localPosition;
            pos.y = Mathf.Sin(v) * 1f;
            transform.localPosition = pos;
        }
    }
}