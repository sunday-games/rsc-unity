using System;
using System.Collections;
using System.Collections.Generic;
using DictSO = System.Collections.Generic.Dictionary<string, object>;

namespace SG.BlockchainPlugin
{
    public class Account : IDictionarizable<Account>
    {
        public static implicit operator bool(Account a) => a != null;

        public Blockchain Blockchain;
        public string Address;
        public string AddressShort => !Address.IsEmpty() && Address.Length > 12 ? Address.CutMiddle(4, 4) : Address;
        public string PublicKey;
        public string Name;
        public decimal Balance;
        public ulong? TransactionCount;

        private string _privateKeyEncrypted;
        public string GetPrivateKey(string pass = null)
        {
            try
            {
                return Cryptography.Decrypt(_privateKeyEncrypted, pass ?? Accounts.Password);
            }
            catch
            {
                return null;
            }
        }
        public void SetPrivateKey(string privateKey, string pass = null) =>
            _privateKeyEncrypted = Cryptography.Encrypt(privateKey, pass ?? Accounts.Password);

        public Account(Blockchain blockchain, string address, string publicKey = null, string privateKey = null, string name = null)
        {
            Blockchain = blockchain;
            Address = address;
            PublicKey = publicKey;
            Name = name;

            if (privateKey.IsNotEmpty())
                SetPrivateKey(privateKey);
        }

        public void OpenUrl() =>
            UI.Helpers.OpenLink(Blockchain.network.explorer.GetAddressURL(Address));

        private DateTime _balanceLastUpdated;
        public IEnumerator Update(float updateRate = 10f, Action<Result<decimal>> callback = null)
        {
            if (_balanceLastUpdated != default && (DateTime.UtcNow - _balanceLastUpdated).TotalSeconds < updateRate)
            {
                var error = $"Account - Update is too frequent. Last update was {(int) (DateTime.UtcNow - _balanceLastUpdated).TotalSeconds} seconds ago";
                Log.Warning(error);
                callback?.Invoke(new Result<decimal>(error));
                yield break;
            }

            yield return (Blockchain.wallet ?? Blockchain.wallets[0]).AccountGetInfo(
                Blockchain,
                Address,
                result =>
                {
                    if (result.Success)
                        SetBalance(result.Value);

                    callback?.Invoke(result);
                });
        }

        public void SetBalance(decimal balance)
        {
            _balanceLastUpdated = DateTime.UtcNow;
            Balance = balance;
        }

        protected Account() { }
        public Account FromDictionary(DictSO data)
        {
            var bcId = data.GetInt("bcId");
            if (bcId == 6)
                bcId = 7; // Temporary crutch, to be removed in May 2022

            Blockchain = Blockchain.Deserialize(bcId);
            Address = data.GetString("address");
            PublicKey = data.GetString("publicKey");
            _privateKeyEncrypted = data.GetString("privateKeyEncrypted");
            Name = data.GetString("name");
            return this;
        }
        public DictSO ToDictionary()
        {
            return new DictSO
            {
                ["bcId"] = Blockchain.id,
                ["address"] = Address,
                ["publicKey"] = PublicKey,
                ["privateKeyEncrypted"] = _privateKeyEncrypted,
                ["name"] = Name,
            };
        }
    }

    public static class Extensions
    {
        public static Account Find(this List<Account> accounts, Account account)
        {
            if (accounts != null)
                foreach (var a in accounts)
                    if (a.Blockchain == account.Blockchain && a.Address.IsEqualIgnoreCase(account.Address))
                        return a;
            return null;
        }

        public static Account Find(this List<Account> accounts, Blockchain blockchain, string address)
        {
            if (accounts != null)
                foreach (var a in accounts)
                    if (a.Blockchain == blockchain && a.Address.IsEqualIgnoreCase(address))
                        return a;
            return null;
        }
    }
}