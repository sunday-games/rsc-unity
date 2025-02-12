#if SG_PAYMENTS
using System;
using System.Collections;
using System.Collections.Generic;
using DictSO = System.Collections.Generic.Dictionary<string, object>;

namespace SG.Payments
{
    public class Order : IDictionarizable<Order>
    {
        public static Action<Order> onCreated;
        public static Action<Order> onPaid;
        public static Action<Order> onTimeout;
        public static Action<Order> onCancelled;
        public static Action<Order> onFailed;
        public static Action<Order> onDelivered;

        public static implicit operator bool(Order order) { return order != null; }

        public string id;
        public Product product;
        public PaymentProvider paymentProvider;
        public Currency currency;
        public DateTime createdDate;
        public DateTime updatedDate;
        // public List<Transaction> transactions = new List<Transaction>();
        public string error;

        public enum Status { NONE, CREATED, PAID, PROCESSING, DELIVERED, TIMEOUT, CANCELLED, FAILED, }
        public Status status;
        public void SetStatus(Status status)
        {
            if (this.status == status)
                return;

            this.status = status;

            updatedDate = DateTime.UtcNow;

            if (status == Status.CREATED)
                onCreated?.Invoke(this);
            else if (status == Status.PAID)
                onPaid?.Invoke(this);
            else if (status == Status.DELIVERED)
                onDelivered?.Invoke(this);
            else if (status == Status.TIMEOUT)
                onTimeout?.Invoke(this);
            else if (status == Status.CANCELLED)
                onCancelled?.Invoke(this);
            else if (status == Status.FAILED)
                onFailed?.Invoke(this);
        }

        public Payer payer;
        public class Payer : IDictionarizable<Payer>
        {
            // public Blockchain blockchain;
            public string account;
            public string email;

            public string id;
            public string countryCode;
            public string name;
            public string surname;

            public Payer() { }
            public Payer FromDictionary(DictSO data)
            {
                //blockchain = data.IsValue("blockchain") ? Blockchain.Deserialize(data["blockchain"].ToString()) : null;
                account = data.IsValue("account") ? data["account"].ToString() : null;
                email = data.IsValue("email") ? data["email"].ToString() : null;
                return this;
            }
            public DictSO ToDictionary()
            {
                return new DictSO {
                    //{ "blockchain", blockchain ? blockchain.name.ToString() : null },
                    { "account", account },
                    { "email", email },
                };
            }

            public override string ToString()
            {
                return ToDictionary().ToDebugString();
            }
        }

        public decimal price => (decimal) product.price;
        public decimal revenue => price - fee;
        public decimal fee => price * paymentProvider.GetFee(product);
        public string priceToView => currency ? ExchangeRates.ConvertFormat(price, Currency.USD, currency) : price.ToString(Currency.USD);
        static TimeSpan timeout = TimeSpan.FromHours(1);
        public TimeSpan age => DateTime.UtcNow - createdDate;
        public bool isTimeout => age > timeout;

        public Order SetId(string id)
        {
            this.id = id;

            createdDate = DateTime.UtcNow;

            SetStatus(Status.CREATED);

            return this;
        }

        public IEnumerator Update()
        {
            //Log.Info($"Order '{id}' - StatusUpdate");
            //Log.Debug(Json.Serialize(ToDictionary()));

            // Don't update the order status if there are pending transaction
            //foreach (var tx in transactions)
            //    if (tx.isPending)
            //    {
            //        Log.Info($"Order '{id}' - StatusUpdate - Will not be updated because there are pending transaction '{tx.hash}'");
            //        yield break;
            //    }

            var download = new Download(Configurator.ApiUrl + $"/api/payment/order/{paymentProvider.Name}/{id}")
                .IgnoreError()
                .SetPlayer(null);

            yield return download.RequestCoroutine();

            if (!download.success || download.responseDict == null)
                yield break;

            //if (payer.account.IsEmpty())
            //{
            //    payer.account = download.responseDict["customer"].ToString();
            //    payer.blockchain = Blockchain.Deserialize(download.responseDict["bcId"].ToInt());
            //}

            updatedDate = download.responseDict.GetDateTime("updatedDate");

            if (download.responseDict.TryGetList("transactions", out List<object> transactions) &&
                transactions.Count > 0 &&
                (transactions[0] as DictSO).GetString("hash").IsNotEmpty())
            {
                SetStatus(Status.DELIVERED); // TODO
            }
            else
            {
                SetStatus(download.responseDict.GetEnum<Status>("status"));
            }

            //if (download.responseDict.IsValue("transactions"))
            //    foreach (var txUpdated in download.responseDict["transactions"].ToClassArray<Transaction>())
            //        if (transactions.GetByHash(txUpdated.hash, out Transaction tx))
            //        {
            //            if (tx.status == Transaction.Status.PENDING && txUpdated.status == Transaction.Status.SUCCESS_SYNCED)
            //                tx.SetStatus(txUpdated.status);
            //        }
            //        else
            //        {
            //            txUpdated.to = payer.account;
            //            if (txUpdated.type == Transaction.Type.MoneyTransfer)
            //            {
            //                txUpdated.function = payer.blockchain.moneyTransferFunction;
            //                txUpdated.money = product.reward.GetCurrencyAmount(payer.blockchain);
            //            }

            //            txUpdated.payload["orderId"] = id;
            //            txUpdated.payload["productId"] = product.id;

            //            transactions.Add(txUpdated);
            //            txUpdated.SetStatus(txUpdated.status, forceEvent: true);
            //        }
        }

        public Order()
        {
            status = Status.NONE;
        }

        public Order FromDictionary(DictSO data)
        {
            id = data["orderId"].ToString();
            product = Configurator.Instance.appInfo.GetProduct(data["productId"].ToString());
            currency = data.TryGetString("currency", out var currencyName) ? Currency.Currencies.GetValue(currencyName) : null;
            paymentProvider = data.IsValue("paymentProvider") ? PaymentProvider.Deserialize(data["paymentProvider"].ToString()) : null; // paymentProvider должен быть всегда, а эта проверка добавлена только для совместимости - позже можно убрать
            createdDate = data.GetDateTime("createdDate");
            updatedDate = data.GetDateTime("updatedDate");
            status = data.GetEnum<Status>("status");
            payer = data.GetClass<Payer>("payer");
            //transactions = data["transactions"].ToClassList<Transaction>();
            return this;
        }
        public DictSO ToDictionary()
        {
            return new DictSO {
                { "orderId", id },
                { "productId", product.id },
                { "currency", currency ? currency.Name : null },
                { "paymentProvider", paymentProvider ? paymentProvider.Name.ToString() : null }, // paymentProvider должен быть всегда, а эта проверка добавлена только для совместимости - позже можно убрать
                { "createdDate", createdDate.ToTimestamp() },
                { "updatedDate", updatedDate.ToTimestamp() },
                { "status", (int)status },
                { "payer", payer != null ? payer.ToDictionary() : null },
                //{ "transactions", transactions.ToObjectList() },
            };
        }
    }
}
#endif