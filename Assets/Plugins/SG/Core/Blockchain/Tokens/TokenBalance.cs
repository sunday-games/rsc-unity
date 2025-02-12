using System;
using System.Linq;
using System.Collections.Generic;

namespace SG.BlockchainPlugin
{
    public class TokenBalance
    {
        public Token Token;
        public long TokenId;

        public Dictionary<Blockchain, decimal> Amounts;

        public decimal Amount;
        public string AmountText => Amount.ToString(Token, decimals: 3, sign: false);
        public bool IsOwned => Amount > 0;

        public Action<Blockchain, decimal> OnUpdated;

        private Dictionary<Blockchain, DateTime> _dates = new Dictionary<Blockchain, DateTime>();
        private TimeSpan _updateFrequency = TimeSpan.FromSeconds(30);
        private string _address;

        public TokenBalance(Token token, string address, double updateFrequency = 30)
        {
            Token = token;
            _updateFrequency = TimeSpan.FromSeconds(updateFrequency);
            _address = address;

            Amounts = new Dictionary<Blockchain, decimal>(Token.Contracts.Count);
            _dates = new Dictionary<Blockchain, DateTime>(Token.Contracts.Count);

            foreach (var blockchain in Token.Contracts.Keys)
            {
                Amounts[blockchain] = default;
                _dates[blockchain] = default;
            }
        }

        public void UpdateBalance(Action<decimal?> callback = null)
        {
            foreach (var blockchain in Token.Contracts.Keys)
                if (blockchain)
                    UpdateBalance(blockchain, callback: callback);
        }

        public void UpdateBalance(Blockchain blockchain, bool force = false, Action<decimal?> callback = null)
        {
            if (!Amounts.ContainsKey(blockchain)) // TODO Maybe: if (!Configurator.production && !Amounts.ContainsKey(blockchain))
            {
                Log.Blockchain.Error($"Balance of {blockchain}.{Token.Name} is not updated becase {blockchain} not supported");
                callback?.Invoke(default);
                return;
            }

            if (!force)
            {
                var timeFromLastUpdate = DateTime.UtcNow - _dates[blockchain];
                if (timeFromLastUpdate < _updateFrequency)
                {
                    Log.Blockchain.Debug($"Balance of {blockchain}.{Token.Name} is not updated becase it already updated {(int) timeFromLastUpdate.TotalSeconds}/{_updateFrequency.TotalSeconds}s ago on {blockchain}");
                    callback?.Invoke(Amounts[blockchain]);
                    return;
                }
            }

            _dates[blockchain] = DateTime.UtcNow;

            Token.GetBalance(blockchain, _address, TokenId,
                    balance =>
                    {
                        if (balance.HasValue)
                        {
                            Amounts[blockchain] = balance.Value;

                            Amount = Amounts.Values.Sum();

                            OnUpdated?.Invoke(blockchain, balance.Value);
                        }

                        callback?.Invoke(balance);
                    })
                .Start();
        }
    }
}