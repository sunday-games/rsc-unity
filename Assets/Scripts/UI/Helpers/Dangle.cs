using UnityEngine;
using System.Collections;
using DG.Tweening;

namespace SG.RSC
{
    public class Dangle : MonoBehaviour
    {
        public Vector3 amplitude;
        public Ease ease = Ease.InOutQuad;
        public float speed = 2f;

        void OnEnable()
        {
            transform.DOKill();
            transform.rotation = Quaternion.Euler(-amplitude);
            transform.DORotate(amplitude, speed).SetEase(ease).SetLoops(-1, LoopType.Yoyo);
        }
    }
}