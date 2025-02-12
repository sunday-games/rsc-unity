using UnityEngine;

namespace SG.UI
{
    public class InspectorNotes : MonoBehaviour
    {
        [TextArea(1, 1000)] public string notes;
    }
}