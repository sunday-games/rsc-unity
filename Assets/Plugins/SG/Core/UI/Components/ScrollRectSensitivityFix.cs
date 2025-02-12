using UnityEngine;
using UnityEngine.UI;

namespace SG.UI
{
    [RequireComponent(typeof(ScrollRect))]
    public class ScrollRectSensitivityFix : MonoBehaviour
    {
        public float macSensitivity = 5f;

        void Awake()
        {
            if (Utils.isMac)
                GetComponent<ScrollRect>().scrollSensitivity = macSensitivity;
        }
    }
}