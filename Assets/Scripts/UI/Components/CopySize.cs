using UnityEngine;

namespace SG
{
    [ExecuteInEditMode]
    public class CopySize : MonoBehaviour
    {
        public enum Mode { Width, WidthSum, Height, HeightSum };
        public Mode mode;
        public float min;
        public float add;
        public RectTransform[] targets;

        RectTransform t;

        void Awake()
        {
            t = transform as RectTransform;
        }

        float widthTarget, widthPrevious;
        float heightTarget, heightPrevious;
        void Update()
        {
            if (targets.Length == 0 || targets[0] == null)
            {
                enabled = false;
            }
            else if (mode == Mode.Width)
            {
                widthTarget = targets[0].sizeDelta.x + add;
                if (widthTarget != widthPrevious)
                {
                    widthPrevious = widthTarget;
                    t.sizeDelta = new Vector2(widthTarget < min ? min : widthTarget, t.sizeDelta.y);
                }
            }
            else if (mode == Mode.Height)
            {
                heightTarget = targets[0].sizeDelta.y + add;
                if (heightTarget != heightPrevious)
                {
                    heightPrevious = heightTarget;
                    t.sizeDelta = new Vector2(t.sizeDelta.x, heightTarget < min ? min : heightTarget);
                }
            }
            else if (mode == Mode.HeightSum)
            {
                heightTarget = add;
                foreach (var target in targets)
                    if (target != null) heightTarget += target.rect.height - target.anchoredPosition.y;

                if (heightTarget != heightPrevious)
                {
                    heightPrevious = heightTarget;
                    t.sizeDelta = new Vector2(t.sizeDelta.x, heightTarget);
                }
            }

        }
    }
}