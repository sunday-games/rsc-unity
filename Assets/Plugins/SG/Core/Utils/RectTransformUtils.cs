using UnityEngine;

namespace SG
{
    public static class RectTransformUtils
    {
        /// <summary>
        /// Checks if this RectTransform is overlaping the other RectTransform in the world space [Z Neutral]
        /// </summary>
        /// <param name="overlaping"></param>
        /// The object overlaping
        /// <param name="overlaped"></param>
        /// the object being overlaped
        /// <returns></returns>
        public static bool WorldSpaceOverlaps(this RectTransform overlaping, RectTransform overlaped)
        {
            var corners = new Vector3[4]; //Cache
                                          //Get worldSpace corners and  then creates a Rect considering the first and the third values are diagonal
            overlaping.GetWorldCorners(corners);
            var overlapingRect = new Rect(corners[0], corners[2] - corners[0]);

            //Reapeats for the other RectTranform
            overlaped.GetWorldCorners(corners);
            var overlapedRect = new Rect(corners[0], corners[2] - corners[0]);

            //Use Rect.Overlaps to do the necessary calculations
            return overlapedRect.Overlaps(overlapingRect, true);
        }
    }
}