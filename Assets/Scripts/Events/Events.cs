using System;
using System.Collections.Generic;

public class Events : Core
{
    public static Event sale;
    public static Event newYear;
    public static Event stValentin;
    public static Event halloween;
    public static Event digitalStar;

    public enum EventTypes { Sale, NewYear, StValentin, Halloween }
    static Dictionary<EventTypes, Event> dict = null;
    public static bool IsActive(EventTypes type) { return dict == null ? false : dict[type].isActive; }
    public static bool IsHaveGift(EventTypes type) { return dict == null ? false : dict[type].isHaveGift; }

    public static void Init()
    {
        sale = new Event(() => sale.endDate > DateTime.Now && DateTime.Now > sale.startDate && Missions.isGoldfishes);

        newYear = new Event(
            () => newYear.endDate > DateTime.Now && DateTime.Now > newYear.startDate && user.collection.Count > 0,
            () => user.isOwned(Cats.Santa));
        newYear.calcMainDate = () => Localization.language == UnityEngine.SystemLanguage.Russian ?
            new DateTime(newYear.endDate.Year, 1, 1) : new DateTime(newYear.startDate.Year, 12, 25);

        stValentin = new Event(
            () => stValentin.endDate > DateTime.Now && DateTime.Now > stValentin.startDate && user.collection.Count > 0,
            () => user.isOwned(Cats.Lady));
        stValentin.calcMainDate = () => new DateTime(newYear.startDate.Year, 2, 14);

        halloween = new Event(
            () => halloween.endDate > DateTime.Now && DateTime.Now > halloween.startDate && user.collection.Count > 0,
            () => user.isOwned(Cats.Jack));
        halloween.calcMainDate = () => new DateTime(newYear.startDate.Year, 10, 31);

        dict = new Dictionary<EventTypes, Event>()
        {
            { EventTypes.Sale, sale },
            { EventTypes.NewYear, newYear },
            { EventTypes.StValentin, stValentin },
            { EventTypes.Halloween, halloween },
        };
    }

    public class Event
    {
        public DateTime endDate = DateTime.MinValue;
        public DateTime startDate = DateTime.MaxValue;
        public TimeSpan timeLeft => endDate - DateTime.Now;

        public Func<DateTime> calcMainDate = null;
        public DateTime mainDate => calcMainDate();

        public Func<bool> condition = null;
        public bool isActive => condition();

        public Func<bool> conditionGift = null;
        public bool isHaveGift => conditionGift == null ? false : conditionGift();

        public List<object> data = null;

        public bool isItemTryDrop = false;
        public bool isItemGet = false;

        public Event(Func<bool> condition, Func<bool> conditionGift = null)
        {
            this.condition = condition;
            this.conditionGift = conditionGift;
        }

        public void TurnOn()
        {
            startDate = DateTime.Now - TimeSpan.FromDays(3);
            endDate = DateTime.Now + TimeSpan.FromDays(3);
        }

        public void TurnOff()
        {
            startDate -= TimeSpan.FromDays(300);
            endDate -= TimeSpan.FromDays(300);
        }
    }
}

