using UnityEngine;
using UnityEngine.EventSystems;

namespace SG.UI
{
    public class Blinds : MonoBehaviour, IPointerClickHandler
    {
        public void Show() => gameObject.SetActive(true);
        public void Hide() => gameObject.SetActive(false);

        public void OnPointerClick(PointerEventData eventData) => UI.Instance.current?.OnEscapeKey();
    }
}