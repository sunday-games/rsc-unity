#if SG_PAYMENTS
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace SG.Payments
{
    public class OrderManager : MonoBehaviour
    {
        public bool Epic;
        public bool Xsolla;
        public bool PayPal;
        public bool CardPay;
        //public bool Mobile;

        public static string Defines()
        {
            var instance = Configurator.Instance.GetComponentInChildren<OrderManager>();

            string defines = "";
            if (instance.Epic && !Configurator.Instance.blockchain)
                defines += "-define:EPIC_GAMES" + Const.lineBreak;
            if (instance.Xsolla)
                defines += "-define:XSOLLA" + Const.lineBreak;
            if (instance.PayPal)
                defines += "-define:PAYPAL" + Const.lineBreak;
            if (instance.CardPay)
                defines += "-define:CARDPAY" + Const.lineBreak;
            //if (instance.Mobile)
            //    defines += "-define:UNITY_PURCHASING" + Const.lineBreak;

            return defines;
        }

        public static Action onOrdersLoaded;

        public static List<Order> orders = new List<Order>();
        public static string ordersPrefs = "SG.Payments.orders";

        [Space]
        public float frequency = 10f;

        public static Order GetOrder(string id)
        {
            foreach (var order in orders)
                if (order.id == id)
                    return order;
            return null;
        }

        public static void Add(Order order)
        {
            if (orders.TryFind(o => o.id == order.id, out Order _))
                return;

            orders.Insert(0, order);
            Save();
        }
        public static void Remove(Order order)
        {
            if (orders.Remove(order))
                Save();
        }
        public static void RemoveAll()
        {
            orders.Clear();
            PlayerPrefs.DeleteKey(ordersPrefs);
        }

        private static void Save()
        {
            PlayerPrefs.SetString(ordersPrefs, Json.Serialize(orders.ToObjectList()));
        }

        private static void Load()
        {
            var ordersJson = PlayerPrefs.GetString(ordersPrefs, "[]");

            Log.Debug("OrderManager - Load: " + ordersJson);

            orders = Json.Deserialize(ordersJson).ToClassList<Order>();

            onOrdersLoaded?.Invoke();
        }

        public void Setup()
        {
            foreach (var providerType in PaymentProvider.GetSupportedProviders())
                (transform.CreateComponent(providerType) as PaymentProvider).Setup();
        }

        public void Init()
        {
            // TODO
            PaymentProvider.All = new();
            if (Epic)
            {
                var provider = GetComponentInChildren<Epic>();
                PaymentProvider.All[provider.Name] = provider;
            }
            if (Xsolla)
            {
                var provider = GetComponentInChildren<Xsolla>();
                PaymentProvider.All[provider.Name] = provider;
            }

            Order.onCreated += Add;
            Order.onFailed += Remove;
            Order.onDelivered += Remove;
            Order.onTimeout += _ => Save();

            Load();
            StopCoroutine(UpdateStatus());
            StartCoroutine(UpdateStatus());

            IEnumerator UpdateStatus()
            {
                while (true)
                {
                    yield return new WaitForSecondsRealtime(frequency);

                    foreach (var order in orders.ToArray())
                        if (order.status == Order.Status.CREATED ||
                            order.status == Order.Status.PAID ||
                            order.status == Order.Status.PROCESSING)
                        {
                            if (order.isTimeout)
                            {
                                if (order.status == Order.Status.CREATED)
                                    Remove(order);

                                order.SetStatus(Order.Status.TIMEOUT);
                            }
                            else if (order.id.IsNotEmpty() && order.paymentProvider.IsLogin())
                            {
                                yield return order.Update();
                            }
                        }
                }
            }
        }
    }
}
#endif