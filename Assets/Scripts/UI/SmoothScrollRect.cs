namespace B1NARY
{
   using DG.Tweening;
   using UnityEngine;
   using UnityEngine.EventSystems;
   using UnityEngine.UI;
   using UI;

   public class SmoothScrollRect : ScrollRect
   {
        public bool SmoothScrolling { get; set; } = true;
        public float SmoothScrollTime { get; set; } = 0.002f;
        new void Awake()
        {
            base.Awake();
            this.content = this.gameObject.transform.GetChild(0).GetComponent<RectTransform>();
            this.viewport = this.gameObject.GetComponent<RectTransform>();
        }
        public override void OnScroll(PointerEventData data)
        {
            if (!IsActive())
                return;
    
    
            if (SmoothScrolling)
            {
                Vector2 positionBefore = normalizedPosition;
                this.DOKill(complete: true);
                base.OnScroll(data);
                Vector2 positionAfter = normalizedPosition;
    
                normalizedPosition = positionBefore;
                this.DONormalizedPos(positionAfter, SmoothScrollTime);
            }
            else
            {
                base.OnScroll(data);
            }
        }
   }  
}