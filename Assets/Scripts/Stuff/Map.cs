﻿using UnityEngine;
using UnityEngine.UI;

namespace SG.RSC
{
    public class Map : MonoBehaviour
    {
        public Image feverImage;
        public GameObject coinSlot;
        public Image coinParent;
        public Image coinsImage;
        public Text coinsText;

        public float minFever = 0.06f;
        public float maxFever = 0.88f;
    }
}