using UnityEngine;

namespace SG.RSC
{
    public class EventVisible : Core
    {
        public Events.EventTypes Event;
        public GameObject[] images;
        public GameObject[] showIfDontHaveGift;

        void OnEnable()
        {
            foreach (var go in images) go.SetActive(Events.IsActive(Event));
            foreach (var go in showIfDontHaveGift) go.SetActive(Events.IsActive(Event) && !Events.IsHaveGift(Event));
        }
    }
}