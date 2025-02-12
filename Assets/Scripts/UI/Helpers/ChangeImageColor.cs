using UnityEngine;
using UnityEngine.UI;

namespace SG.RSC
{
    public class ChangeImageColor : MonoBehaviour
    {
        public float speed = 0.01f;

        Image image;
        float g;
        float gD;
        float b;
        float bD;

        void Awake()
        {
            image = GetComponent<Image>();
            g = Random.value;
            gD = g > 0.5f ? speed : -speed;

            b = Random.value;
            bD = g > 0.5f ? speed : -speed;
        }

        void Update()
        {
            if (gD > 0f && g < 1f) g += gD;
            else if (gD > 0f && g >= 1f) { gD = -gD; g += gD; }
            if (gD < 0f && g > 0f) g += gD;
            else if (gD < 0f && g <= 0f) { gD = -gD; g += gD; }

            if (bD > 0f && b < 1f) b += bD;
            else if (bD > 0f && b >= 1f) { bD = -bD; b += bD; }
            if (bD < 0f && b > 0f) b += bD;
            else if (bD < 0f && b <= 0f) { bD = -bD; b += bD; }

            image.color = new Color(1f, g, b, 1f);
        }
    }
}