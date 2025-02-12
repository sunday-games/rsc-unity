using UnityEngine;

namespace SG.RSC
{
    [ExecuteInEditMode]
    public class AncorTo : MonoBehaviour
    {
        public enum Alignment { UpperLeft, UpperCenter, UpperRight };
        public Alignment alignment;
        public Vector2 shift;
        public RectTransform target;

        RectTransform t;

        void Awake()
        {
            t = transform as RectTransform;
        }

        Vector2 ancorTarget, ancorPrevious;
        void Update()
        {
            if (target == null) return;

            if (alignment == Alignment.UpperLeft)
            {
                ancorTarget = shift + target.anchoredPosition + new Vector2(target.rect.xMin, target.rect.yMax);
                if (ancorTarget != ancorPrevious)
                {
                    ancorPrevious = ancorTarget;
                    t.anchoredPosition = ancorTarget;
                }
            }
            else if (alignment == Alignment.UpperCenter)
            {
                ancorTarget = shift + target.anchoredPosition + new Vector2(target.rect.x, target.rect.yMax);
                if (ancorTarget != ancorPrevious)
                {
                    ancorPrevious = ancorTarget;
                    t.anchoredPosition = ancorTarget;
                }
            }
            else if (alignment == Alignment.UpperRight)
            {
                ancorTarget = shift + target.anchoredPosition + new Vector2(target.rect.xMax, target.rect.yMax);
                if (ancorTarget != ancorPrevious)
                {
                    ancorPrevious = ancorTarget;
                    t.anchoredPosition = ancorTarget;
                }
            }
        }
    }
}