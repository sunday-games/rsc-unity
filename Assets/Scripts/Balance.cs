using UnityEngine;
using System;

public class Balance : Core
{
    void Awake()
    {
        if (build.premium)
        {
            reward.getCoinChance *= 1.5f;
            events.hatsForGift /= 2;
            events.heartsForGift /= 2;
            events.batsForGift /= 2;
        }
    }

    [Tooltip("Длинна сессии в секундах")]
    public int baseTime;

    [Tooltip("Бонус к времени сессии за каждую пройденную лигу")]
    public int leagueTimeBonus;

    [Tooltip("Количество котов в игре")]
    public int catsInGame;

    [Tooltip("Максимальное расстояние до следующего кота в цепочке")]
    public float maxDistanceToNextCat;

    [Tooltip("Базовое значение очков, скорость их прироста")]
    public int baseScore;

    [Tooltip("Предлашать оставить отзыв раз в столько то игр")]
    public int showRateAppEveryGame;

    public Fever fever;
    [Serializable]
    public class Fever
    {
        [Tooltip("Скорость снижения диско")]
        public float speedDown;

        [Tooltip("Скорость повышения диско")]
        public float speedUp;

        [Tooltip("Коэф снижения скорость повышения диско при ее вызове ")]
        public float speedUpReducer;

        [Tooltip("Продолжительность диско")]
        public float time;
    }

    [Tooltip("Сколько опыта нужно для levelup котика")]
    public int[] catLevelsExp = new int[9];

    public Reward reward;
    [Serializable]
    public class Reward
    {
        [Tooltip("Шанс получить золотую рыбку")]
        public float getCoinChance;
        [Tooltip("Сколько получит золотых рыбок игрок за каждую собранную в игре")]
        public int coinValue;
        [Tooltip("Начальное количество золотых рыбок")]
        public int startCoins;
        [Tooltip("Бонус за логин в Facebook")]
        public int coinsForFacebookLogin;
        [Tooltip("Количество золотых рыбок за инвайт")]
        public int coinsForInvite;
        [Tooltip("Количество золотых рыбок за шару рекорда")]
        public int coinsForShareHighscore;
        [Tooltip("Количество золотых рыбок за шару повишения в лиге")]
        public int coinsForShareLeagueUp;
        [Tooltip("Количество золотых рыбок за шару нового котика")]
        public int coinsForShareCat;
        [Tooltip("Количество золотых рыбок за шару результатов турнира")]
        public int coinsForShareTournamentResult;
        [Tooltip("Количество золотых рыбок за просмотр рекламы в зависимости от уровня 0-9,10-19,20-29,30+")]
        public int[] coinsForAdViewByLevel = new int[4];
        public int coinsForAdView
        {
            get
            {
                if (user.level < 10) return coinsForAdViewByLevel[0];
                else if (user.level < 20) return coinsForAdViewByLevel[1];
                else if (user.level < 30) return coinsForAdViewByLevel[2];
                else return coinsForAdViewByLevel[3];
            }
        }
        [Tooltip("Количество золотых рыбок в малом аквариуме, который дарится за levelup. Это значение множится с каждым разом")]
        public int coinsForLevelUp;
        [Tooltip("Количество сарделек в подарке, который дарится за levelup")]
        public int sausageForLevelUp;
        [Tooltip("В промежутке времени от 0 до этого числа может пыпасть на поле эвентовый предмет")]
        public float timeDropEventItem = 10f;

        public Cats[] superCatsInSimpleBox = new Cats[] {
            Cats.Boom, Cats.Cap, Cats.Snow, Cats.Zen, Cats.Disco, Cats.King, Cats.Mage, Cats.Joker, Cats.Loki, Cats.Flint };

        public Cats[] superCatsInPremiumBox = new Cats[] {
            Cats.Boom, Cats.Cap, Cats.Snow, Cats.Zen, Cats.Disco, Cats.King, Cats.Mage, Cats.Joker, Cats.Loki, Cats.Flint, Cats.Lady, Cats.Santa };
    }

    public Shop shop;
    [Serializable]
    public class Shop
    {
        [Tooltip("Стоимость всех котов умножитется на этот коэфициент и в итоге получается содержание аквариума")]
        public float goldfish = 3f;
        public int sausages = 3;
    }

    public Events events;
    [Serializable]
    public class Events
    {
        public int hatsPurchase = 50;
        public int hatsForGift = 100;

        [Space(10)]
        public int heartsPurchase = 50;
        public int heartsForGift = 100;

        [Space(10)]
        public int batsPurchase = 50;
        public int batsForGift = 100;
    }

    public FireworkChain fireworkChain;
    [Serializable]
    public class FireworkChain
    {
        [Tooltip("Минимальная длинна цепочки для вызова небольшой петарды")]
        public int boomSmall;
        [Tooltip("Минимальная длинна цепочки для вызова рокеты")]
        public int rocket;
        [Tooltip("Минимальная длинна цепочки для вызова большой петарды")]
        public int boomBig;
        [Tooltip("Минимальная длинна цепочки для вызова цветной петарды")]
        public int color;
    }

    [Tooltip("Сколько нужно копить на получение следующего множителя")]
    public int[] multiplierGetting = new int[9];

    [Tooltip("При какой длинне цепочки какой голос воспроизводить")]
    public int[] voiceCombo = new int[5];

    public RentCat rentCat;
    [Serializable]
    public class RentCat
    {
        [Tooltip("После какого уровня будет предлагаться кот на прокат")]
        public int startLevel = 5;

        [Tooltip("Раз в сколько сессий будет предлагаться кот на прокат")]
        public int frequency = 20;

        public Cats[] cats = new Cats[] {
            Cats.Boom,
            Cats.Cap,
            Cats.Snow,
            Cats.Zen,
            Cats.Mage,
            Cats.Lady,
            Cats.Raiden,
            Cats.Orion,
            Cats.Loki,
            Cats.King,
            Cats.Joker,
            Cats.Disco,
            Cats.Flint
        };
    }

    public TimeSpan freeSpinTime = new TimeSpan(8, 0, 0);

    public VerifyTime verifyTime;
    [Serializable]
    public class VerifyTime
    {
        [Tooltip("Точность проверки времени (в минутах)")]
        public int accuracy = 180;
        [Tooltip("Частота проверки времени (в минутах)")]
        public int frequency = 60;
    }
}


