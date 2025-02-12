using UnityEngine;

namespace SG.UI
{
    [ExecuteInEditMode]
    public class CopySize : MonoBehaviour
    {
        public enum Mode { Width, WidthSum, Height, HeightSum, WidthHeight };
        public Mode mode;
        public float min;
        public float max = float.MaxValue;
        public float add;
        public RectTransform[] targets = new RectTransform[0];

        RectTransform rt;

        void Awake()
        {
            rt = transform as RectTransform;
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
                    rt.sizeDelta = new Vector2(Mathf.Clamp(widthTarget, min, max), rt.sizeDelta.y);
                }
            }
            else if (mode == Mode.WidthSum)
            {
                widthTarget = add;
                foreach (var target in targets)
                    if (target != null) widthTarget += target.rect.width - target.anchoredPosition.x;

                if (widthTarget != widthPrevious)
                {
                    widthPrevious = widthTarget;
                    rt.sizeDelta = new Vector2(Mathf.Clamp(widthTarget, min, max), rt.sizeDelta.y);
                }
            }
            else if (mode == Mode.Height)
            {
                heightTarget = targets[0].sizeDelta.y + add;
                if (heightTarget != heightPrevious)
                {
                    heightPrevious = heightTarget;
                    rt.sizeDelta = new Vector2(rt.sizeDelta.x, Mathf.Clamp(heightTarget, min, max));
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
                    rt.sizeDelta = new Vector2(rt.sizeDelta.x, Mathf.Clamp(heightTarget, min, max));
                }
            }
            else if (mode == Mode.WidthHeight)
            {
                heightTarget = targets[0].sizeDelta.y + add;
                widthTarget = targets[0].sizeDelta.x + add;
                if (heightTarget != heightPrevious || widthTarget != widthPrevious)
                {
                    heightPrevious = heightTarget;
                    widthPrevious = widthTarget;
                    rt.sizeDelta = new Vector2(Mathf.Clamp(widthTarget, min, max), Mathf.Clamp(heightTarget, min, max));
                }
            }
        }
    }
}