using UnityEngine;
using UnityEngine.UI;

namespace SG.RSC
{
    public class BasicCatAnimation : MonoBehaviour
    {
#if GAF
    public GAF.Core.GAFMovieClip clip;
#endif
        public int _frameHalf = 15;
        public uint frameHalf { get { return (uint)_frameHalf; } }
        public uint frameEnd
        {
            get
            {
#if GAF
            return clip.getFramesCount() - 1;
#else
                return 0;
#endif
            }
        }

        public Text multiplierText;
    }
}