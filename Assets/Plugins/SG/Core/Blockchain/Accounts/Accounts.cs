using System.Collections.Generic;
using UnityEngine;

namespace SG.BlockchainPlugin
{
    public static class Accounts
    {
        public static Account AccountCreate(Blockchain blockchain, string name, string privateKey = null)
        {
#if SG_BLOCKCHAIN
            var account = NethereumManager.AccountCreate(blockchain, name, privateKey);

            if (account == null && privateKey != null)
                SundayWallet.UI.windows.error.Open("errorInvalidPrivateKey");

            return account;
#else
            return null;
#endif
        }

        public static List<Account> List = new List<Account>();
        public static bool AccountAdd(Account account)
        {
            if (List.Find(account))
                return false;

            List.Add(account);
#if SG_BLOCKCHAIN
            if (SundayWallet.Settings.solidityAccountDuplicateMode)
                foreach (var blockchain in Wallet.GetWallet<SundayWallet>().supportedBlockchains)
                    if (account.Blockchain != blockchain)
                        List.Add(new Account(blockchain, account.Address, account.PublicKey, account.GetPrivateKey(), account.Name));
#endif
            AccountsSave();

            return true;
        }
        public static void AccountChangeName(Account account, string newName)
        {
            foreach (var a in List.FindAll(a => a.Address == account.Address))
                a.Name = newName;

            AccountsSave();
        }
        public static bool AccountRemove(Account account)
        {
            var removed = List.Remove(account);
            AccountsSave();
            return removed;
        }
        public static void AccountRemoveAll()
        {
            List.Clear();
            PlayerPrefs.DeleteKey(PREF_ACCOUNTS);
        }

        private const string PREF_ACCOUNTS = "SG.BlockchainPlugin.SundayWallet.accounts";
        public static void AccountsSave()
        {
            PlayerPrefs.SetString(PREF_ACCOUNTS, Json.Serialize(List.ToObjectList()));
        }
        public static void AccountsLoad()
        {
            List = Json.Deserialize(PlayerPrefs.GetString(PREF_ACCOUNTS, "[]")).ToClassList<Account>();
#if SG_BLOCKCHAIN
            if (SundayWallet.Settings.solidityAccountDuplicateMode)
                foreach (var account in List.ToArray())
                    foreach (var blockchain in Wallet.GetWallet<SundayWallet>().supportedBlockchains)
                        if (account.Blockchain != blockchain && !List.Find(blockchain, account.Address))
                            List.Add(new Account(blockchain, account.Address, account.PublicKey, account.GetPrivateKey(), account.Name));
#endif
            //if (!Configurator.production && Accounts.Count == 0)
            //    foreach (var account in Resources.FindObjectsOfTypeAll<TestAccount>())
            //        account.AddToWalletButton_OnClick();
        }

        #region PIN
        public static string Password => "fsd" + 23 * 11 + "uFgK" + 13 * 47 + "oDrK" + PIN;

        public static string PIN = string.Empty;

        public static void SetPIN(string pin)
        {
            foreach (var account in List)
            {
                var privateKey = account.GetPrivateKey();
                account.SetPrivateKey(privateKey, Password + pin);
            }

            PIN = pin;

            AccountsSave();
        }

        public static bool IsNeedPIN => List.Count > 0 && List[0].GetPrivateKey().IsEmpty();

        public static bool IsCorrectPIN(string pin) =>
            List.Count > 0 && List[0].GetPrivateKey(Password + pin).IsNotEmpty();
        #endregion
    }
}