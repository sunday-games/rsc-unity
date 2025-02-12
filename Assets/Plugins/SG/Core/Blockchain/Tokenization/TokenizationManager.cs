using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace SG.BlockchainPlugin
{
    public static class TokenizationManager
    {
        private static List<Tokenization> _tokenizations;

        public static void Init(Token[] tokens)
        {
            Transaction.OnStatusUpdated += Remove;

            foreach (var token in tokens)
                token.GetTokenizator().Setup();

            Load();
        }

        public static void Add(Tokenization tokenization)
        {
            if (_tokenizations.TryFind(t => t.hash == tokenization.hash, out Tokenization _))
                return;

            _tokenizations.Add(tokenization);
            Log.Blockchain.Info("Tokenization - Added");

            Save();
        }

        public static void Remove(Transaction tx)
        {
            if (_tokenizations.Count > 0 &&
                tx.status == Transaction.Status.SUCCESS_SYNCED &&
                _tokenizations.TryFind(item => item.txHash == tx.Hash, out Tokenization tokenization))
                Remove(tokenization);
        }

        public static void Remove(Tokenization tokenization)
        {
            _tokenizations.Remove(tokenization);
            Log.Blockchain.Info("Tokenization - Removed");

            Save();
        }

        private const string PREF_KEY = "SG.Tokenizations";
        public static void Save()
        {
            PlayerPrefs.SetString(PREF_KEY, JsonConvert.SerializeObject(_tokenizations));
            PlayerPrefs.Save();

            Log.Blockchain.Info($"Tokenization - Saved {_tokenizations.Count} tokenizations");
        }

        public static void Load()
        {
            _tokenizations = PlayerPrefs.HasKey(PREF_KEY) ?
                JsonConvert.DeserializeObject<List<Tokenization>>(PlayerPrefs.GetString(PREF_KEY)) :
                new List<Tokenization>();

            Log.Blockchain.Info($"Tokenization - Loaded {_tokenizations.Count} tokenizations");

            foreach (var tokenization in _tokenizations.ToArray())
            {
                if (tokenization.txHash.IsNotEmpty() &&
                    (TransactionManager.archive.TryFind(tx => tx.Hash == tokenization.txHash, out Transaction tx) ||
                    TransactionManager.transactions.TryFind(tx => tx.Hash == tokenization.txHash, out tx)))
                {
                    if (tx.status == Transaction.Status.PENDING || tx.status == Transaction.Status.SUCCESS)
                    {
                        continue;
                    }
                    else if (tx.status == Transaction.Status.SUCCESS_SYNCED)
                    {
                        Remove(tokenization);
                        continue;
                    }
                    else
                    {
                        tokenization.ShowNotification();
                    }
                }
                else
                {
                    tokenization.ShowNotification();
                }
            }
        }
    }
}