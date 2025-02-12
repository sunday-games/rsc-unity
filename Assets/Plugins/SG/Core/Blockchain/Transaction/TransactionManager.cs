using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace SG.BlockchainPlugin
{
    public static class TransactionManager
    {
        public static Action onUpdated;
        public static List<Transaction> transactions = new List<Transaction>();
        public static List<Transaction> archive = new List<Transaction>();

        public static List<string> TransactionIgnoreSync;

        private const string TX_PREFS_KEY = "Ox.BlockchainPlugin.transactions";
        private const string ARCHIVE_PREFS_KEY = "SG.BlockchainPlugin.archive";

        public static void Init()
        {
            TransactionIgnoreSync = new List<string>
            {
                TransferTransaction.TYPE,
                Token.ApproveTransaction.TYPE,
                Token.BurnTransaction.TYPE,
                Token.MintTransaction.TYPE,
                Uniswap.SwapTransaction.TYPE,
            };

            Transaction.OnStatusUpdated += (tx) =>
            {
                if (tx.isPending)
                    Add(tx);
                else if (tx.status == Transaction.Status.FAIL || tx.status == Transaction.Status.TIMEOUT)
                    Save();
            };

            Transaction.OnProgressUpdated += (tx) => Save();

            Transaction.OnReplaced += (oldHash, tx) => Save();

            Load();
            BlockchainManager.Instance.StopCoroutine(UpdateStatus());
            BlockchainManager.Instance.StartCoroutine(UpdateStatus());
        }

        private static IEnumerator UpdateStatus()
        {
            while (true)
            {
                yield return new WaitForSecondsRealtime(5f);

                foreach (var tx in transactions.ToArray())
                    if (DateTime.UtcNow >= tx.statusLastUpdated + tx.Blockchain.network.BlockCreationTime)
                    {
                        if (tx.isPending)
                        {
                            if (tx.Blockchain.wallet && tx.Blockchain.loginedAccount)
                                yield return tx.Blockchain.wallet.TransactionSetStatus(tx);

                            if (tx.isPending && tx.isTimeout)
                                tx.SetStatus(Transaction.Status.TIMEOUT);
                        }
                        else if (tx.status == Transaction.Status.SUCCESS)
                        {
                            if (TransactionIgnoreSync.Contains(tx.Type))
                            {
                                tx.SetStatus(Transaction.Status.SUCCESS_SYNCED);
                                continue;
                            }

                            Transaction.OnSyncRequired?.Invoke(tx);
                        }
                        else if (tx.status == Transaction.Status.SUCCESS_SYNCED)
                        {
                            MoveToArchive(tx.Hash);
                        }
                    }
            }
        }

        public static Transaction IsTransactionPending(params string[] types)
        {
            foreach (var transaction in transactions)
                foreach (var type in types)
                    if (transaction.Type == type && transaction.isPending)
                        return transaction;
            return null;
        }

        public static Transaction IsTransactionWating(params string[] types)
        {
            foreach (var transaction in transactions)
                foreach (var type in types)
                    if (transaction.Type == type && transaction.isWating)
                        return transaction;
            return null;
        }

        public static bool Add(Transaction tx)
        {
            if (transactions.TryFind(t => t.Hash == tx.Hash, out Transaction _))
                return false;

            transactions.Insert(0, tx);
            Save();
            return true;
        }
        public static bool Remove(Transaction tx)
        {
            var result = transactions.Remove(tx);

            if (!result)
                result = archive.Remove(tx);

            if (result)
                Save();

            return result;
        }
        public static bool MoveToArchive(string txHash)
        {
            if (!transactions.TryFind(t => t.Hash == txHash, out Transaction tx))
                return false;

            archive.Add(tx);

            return Remove(tx);
        }
        public static void RemoveAll()
        {
            transactions.Clear();
            PlayerPrefs.DeleteKey(TX_PREFS_KEY);

            archive.Clear();
            PlayerPrefs.DeleteKey(ARCHIVE_PREFS_KEY);
        }

        private static void Save()
        {
            PlayerPrefs.SetString(TX_PREFS_KEY, Json.Serialize(transactions.ToObjectList()));
            PlayerPrefs.SetString(ARCHIVE_PREFS_KEY, Json.Serialize(archive.ToObjectList()));
            onUpdated?.Invoke();
        }
        private static void Load()
        {
            transactions = Json.Deserialize(PlayerPrefs.GetString(TX_PREFS_KEY, "[]")).ToClassList<Transaction>();
            Log.Blockchain.Info($"Transactions - Loaded {transactions.Count} transactions");

            archive = Json.Deserialize(PlayerPrefs.GetString(ARCHIVE_PREFS_KEY, "[]")).ToClassList<Transaction>();
            Log.Blockchain.Info($"Transactions - Loaded {archive.Count} archive transactions");
        }
    }
}