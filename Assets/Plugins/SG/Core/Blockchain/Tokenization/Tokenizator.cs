using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using DictSO = System.Collections.Generic.Dictionary<string, object>;

namespace SG.BlockchainPlugin
{
    public static class TokenizatorEx
    {
        public static Tokenizator GetTokenizator(this Currency currency) =>
            Resources.Load<Tokenizator>($"Tokens/{currency.Name}_Tokenizator");
    }

    [CreateAssetMenu(menuName = "Sunday/Blockchain/" + nameof(Tokenizator), fileName = nameof(Tokenizator), order = 0)]
    public class Tokenizator : ScriptableObject
    {
        [SerializeField] private TokenView _tokenView;
        public Token Token => _tokenView.Token;

        [SerializeField] private BlockchainAddressDictionary _addresses, _addressesDebug;
        public BlockchainAddressDictionary Addresses => Configurator.production ? _addresses : _addressesDebug;

        private Dictionary<Blockchain, SmartContract> _contracts = new Dictionary<Blockchain, SmartContract>();

        public bool TryGetFunction(Blockchain blockchain, string functionName, out SmartContract.Function function) =>
            _contracts[blockchain].Functions.TryGetValue(functionName, out function);

        public void Setup()
        {
            _contracts = Addresses.ToContracts("Distributor");
        }

        public IEnumerator Tokenize(Download.ITokenPlayer from, Blockchain blockchain, decimal amount, string recipient = null, Action<Transaction> callback = null)
        {
            if (!TryGetFunction(blockchain, "nonces", out var function))
            {
                Log.Blockchain.Error(blockchain + " is not supported by " + Token.Name);
                callback?.Invoke(null);
                yield break;
            }

            if (!blockchain.loginedAccount)
            {
                Log.Blockchain.Warning($"Please login first");
                callback?.Invoke(null);
                yield break;
            }

            if (recipient == null)
                recipient = blockchain.loginedAccount.Address;

            int? recipientNonce = null;
            yield return function.Read<int>(
                new DictSO { ["address"] = recipient },
                result => recipientNonce = result.Success ? result.Value : null);

            if (!recipientNonce.HasValue)
            {
                Log.Blockchain.Error("Fail to read the nonce");
                callback?.Invoke(null);
                yield break;
            }

            var download = new Download(Configurator.ApiUrl + $"/api/{Token.Name.ToLower()}/tokenize",
                    new DictSO
                    {
                        ["bcId"] = blockchain.id,
                        ["executor"] = blockchain.loginedAccount.Address,
                        ["recipient"] = recipient,
                        ["count"] = recipientNonce,
                        ["amount"] = Token.ToMin(amount),
                    })
                .SetLoadingName("Tokenize " + Token.Name)
                .SetPlayer(from);

            yield return download.RequestCoroutine();

            if (!download.success)
            {
                callback?.Invoke(null);
                yield break;
            }

            var tokenization = JsonConvert.DeserializeObject<Tokenization>(download.responseText)
                .SetHash(Token);
            TokenizationManager.Add(tokenization);

            if (recipientNonce != tokenization.count)
            {
                Log.Error($"Tokenization failed: nonce {recipientNonce} != count {tokenization.count}");
                callback?.Invoke(null);
                yield break;
            }

            yield return tokenization.MakeTransaction(callback);
        }

        public IEnumerator Detokenize(Blockchain blockchain, decimal amount, ulong recipientId, Action<Transaction> callback = null)
        {
            yield return blockchain.wallet.TransactionSignAndSend(
                new DetokenizeTransaction(Token, blockchain, recipientId, amount),
                callback);
        }

        public IEnumerator SetAuthority(Token token, Blockchain blockchain, string authority, Action<Transaction> callback = null)
        {
            if (!TryGetFunction(blockchain, "setAuthority", out var function))
            {
                Log.Blockchain.Error(blockchain + "is not supported by " + Token.Name);
                yield break;
            }

            if (!blockchain.loginedAccount || !blockchain.loginedAccount.Address.IsEqualIgnoreCase(token.OwnerAddress))
            {
                Log.Blockchain.Warning("Wrong address. Required: " + token.OwnerAddress);
                yield break;
            }

            var transaction = new Transaction("TOKENIZE_SET_AUTHORITY", payload: new DictSO { ["tokenName"] = token.Name });
            transaction.SetFunction(function, new DictSO { ["newAuthority"] = authority });

            yield return blockchain.wallet.TransactionSignAndSend(transaction, callback);
        }

        public IEnumerator GetAuthority(Blockchain blockchain, Action<Result<string>> callback = null)
        {
            if (!TryGetFunction(blockchain, "authority", out var function))
            {
                Log.Blockchain.Error(blockchain + "is not supported by " + Token.Name);
                yield break;
            }

            yield return function.Read<string>(new DictSO(), callback);
        }

        [Header("Functions")]
        public Blockchain.Names BlockchainName;
        protected Blockchain Blockchain => BlockchainName.ToBlockchain();

        [UI.Button("SetAuthority_OnClick")] public bool setAuthority;
        public void SetAuthority_OnClick()
        {
            if (!Blockchain || !Blockchain.loginedAccount)
            {
                Log.Blockchain.Warning($"Login first");
                return;
            }

            SetAuthority(_tokenView.Token, Blockchain, Token.AuthorityAddress).Start();
        }

        [UI.Button("GetAuthority_OnClick")] public bool getAuthority;
        public void GetAuthority_OnClick()
        {
            if (!Blockchain || !Blockchain.loginedAccount)
            {
                Log.Blockchain.Warning($"Login first");
                return;
            }

            GetAuthority(Blockchain,
                result =>
                {
                    Log.Info($"Authority is {(result.Value == Token.AuthorityAddress ? "right" : "wrong")}: {result.Value}");
                })
                .Start();
        }

        [UI.Button("GrantMinterRole_OnClick")] public bool grantMinterRole;
        public void GrantMinterRole_OnClick()
        {
            if (!Blockchain || !Blockchain.loginedAccount)
            {
                Log.Blockchain.Warning($"Login first");
                return;
            }

            Token.GrantMinterRole(Blockchain, _contracts[Blockchain].Address).Start();
        }
    }

    public class TokenizeTransaction : Transaction
    {
        public const string TYPE = "TOKENIZE";

        public TokenizeTransaction(Token token, Tokenization tokenization) :
            base(TYPE, payload: new DictSO { ["tokenName"] = token.Name, ["tokenization"] = tokenization.hash })
        {
            if (token.GetTokenizator().TryGetFunction(tokenization.blockchain, "withdraw", out var function))
                SetFunction(function, tokenization.GetContent());
        }
    }

    public class DetokenizeTransaction : Transaction
    {
        public const string TYPE = "DETOKENIZE";

        public DetokenizeTransaction(Token token, Blockchain blockchain, ulong recipientId, decimal amount) :
            base(TYPE, payload: new DictSO { ["tokenName"] = token.Name, ["recipient"] = recipientId })
        {
            SetFunction(token.Contracts[blockchain].Functions["burn"],
                new DictSO { ["amount"] = token.ToMin(amount), ["reason"] = "Recipient=" + recipientId });
        }
    }

    [Serializable]
    public class Tokenization
    {
        public string bcId;
        [JsonIgnore] public Blockchain blockchain => Blockchain.Deserialize(bcId);

        public string recipient;
        public int count;
        public long amount;
        public long deadline;

        public Permission permission;
        [Serializable]
        public class Permission
        {
            public int v;
            public string r;
            public string s;
        }

        public DictSO GetContent() => new DictSO()
        {
            ["recipient"] = recipient,
            ["amount"] = amount,
            ["deadline"] = deadline,
            ["v"] = permission.v,
            ["r"] = permission.r,
            ["s"] = permission.s,
        };

        public string hash;
        public string tokenName;
        [JsonIgnore] public Token token => BlockchainManager.GetToken(tokenName);
        public string txHash;

        public Tokenization SetHash(Token token)
        {
            tokenName = token.Name;
            hash = Utils.MD5(bcId + tokenName + count);
            return this;
        }

        public void ShowNotification()
        {
            SG.UI.UI.Instance.Notifications.NotificationCreate(
                hash,
                "transfer".Localize(),
                "finishTokenization".Localize(
                    token.FromMin(amount).ToString(token, decimals: 6),
                    recipient.CutMiddle(4, 4),
                    deadline.ToDateTime().ToLocalTime().ToString("MMM d, HH:mm")),
                buttonData1: new SG.UI.ButtonData("submit".Localize(), () => MakeTransaction().Start()));
        }

        public IEnumerator MakeTransaction(Action<Transaction> callback = null)
        {
            yield return blockchain.wallet.TransactionSignAndSend(
                new TokenizeTransaction(token, this),
                tx =>
                {
                    if (tx.Hash.IsNotEmpty())
                    {
                        txHash = tx.Hash;
                        TokenizationManager.Save();
                    }
                    else
                    {
                        ShowNotification();
                    }

                    callback?.Invoke(tx);
                });
        }
    }
}