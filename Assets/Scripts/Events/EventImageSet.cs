using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Image))]
public class EventImageSet : Core
{
    public EventSprite[] eventSprites;
    [System.Serializable]
    public class EventSprite
    {
        public Events.EventTypes Event;
        public Sprite Sprite;
    }

    void OnEnable()
    {
        foreach (var eventSprite in eventSprites)
            if (Events.IsActive(eventSprite.Event))
            {
                GetComponent<Image>().sprite = eventSprite.Sprite;
                break;
            }
    }
}
