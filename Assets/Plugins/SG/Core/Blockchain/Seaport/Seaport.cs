using System;
using System.Numerics;
using System.Collections;
using System.Collections.Generic;
using DictSO = System.Collections.Generic.Dictionary<string, object>;
using Newtonsoft.Json;
#if SG_BLOCKCHAIN
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.ABI.Model;
#endif

namespace SG.BlockchainPlugin
{
    public static class OrderExtencions
    {
        public static bool IsListing(this Seaport.Order order) => order.type == Seaport.Order.Type.SALE;
        public static bool IsOffer(this Seaport.Order order) => order.type == Seaport.Order.Type.PURCHASE;

        public static string Localize(this Seaport.Order.Type type) =>
            (type == Seaport.Order.Type.SALE ? "listing" : "offer").Localize();
    }

    public static class Seaport
    {
        [Serializable]
        public class Order
        {
            public long id; // Server id

            [JsonProperty("bcId")] public Blockchain.Names blockchainName;
            [JsonIgnore] public Blockchain blockchain;

            [JsonProperty("orderType")] public Type type; public enum Type { SALE, PURCHASE }

            [JsonProperty("offerer")] public string seller;
            [JsonProperty("receiver")] public string buyer;
            [JsonIgnore]
            public string Offerer
            {
                get => this.IsListing() ? seller : buyer;
                set { if (this.IsListing()) seller = value; else buyer = value; }
            }

            public Status status; public enum Status { ACTIVE, CANCELED, FULFILLED }

            public long startTime;
            public long endTime;

            [JsonProperty("offerToken")] public string nftTokenName;
            [JsonProperty("offerId")] public long nftId;
            [JsonProperty("offerAmount")] public long nftAmount;
            [JsonIgnore] public Token nftToken;
            [JsonIgnore] public NFT nft;
            [JsonIgnore] public string nftTokenAddress;

            [JsonProperty("considerationToken")] public string coinTokenName;
            [JsonProperty("considerationAmount")] public long coinAmount;
            [JsonIgnore] public Token coinToken;
            [JsonIgnore] public string coinTokenAddress;
            [JsonIgnore] public BigInteger coinFeeAmount;
            [JsonIgnore] public decimal coinPrice;
            [JsonIgnore] public decimal coinPricePerNFT;
            [JsonIgnore] public decimal usdPricePerNFT;

            [JsonIgnore] public string coinPriceToString => coinPrice.ToStringFormated2(coinToken);
            [JsonIgnore] public string coinPricePerUnitToString => coinPricePerNFT.ToStringFormated2(coinToken);

            // public string considerationFeeAmount; // TODO now it is constant but it should be var
            // public string considerationFeeBeneficiary; // TODO now it is constant but it should be var

            public long salt;

            public long counter;

            public string signature;
#if SG_BLOCKCHAIN
            public IEnumerator IsValid(bool checkEstimate = false, Action<bool> callback = null)
            {
                //var valid = false;

                //var operatorAddress = Seaport.Addresses[blockchain.name];

                //if (this.IsListing())
                //{
                //    var isEnoughNFT = false;
                //    yield return nftToken.GetBalance(blockchain, seller, nftId,
                //        nftBalance => isEnoughNFT = nftBalance >= nftAmount);

                //    if (isEnoughNFT)
                //        yield return nftToken.GetIsApproved(blockchain, seller, operatorAddress, nftAmount,
                //            nftApproved => valid = nftApproved);
                //}
                //else if (this.IsOffer() && buyer.IsNotEmpty()) // TODO
                //{
                //    var isEnoughCoins = false;
                //    yield return coinToken.GetBalance(blockchain, buyer, callback:
                //        coinBalance => isEnoughCoins = coinBalance >= coinPrice);

                //    if (isEnoughCoins)
                //        yield return coinToken.GetIsApproved(blockchain, buyer, operatorAddress, coinPrice,
                //            coinApproved => valid = coinApproved);
                //}

                //if (checkEstimate && valid)
                //{
                //    var tx = new FulfillBasicOrderTransaction(this);
                //    yield return blockchain.wallet.TransactionEstimate(tx);
                //    valid = tx.error.IsEmpty();
                //}

                var tx = new FulfillBasicOrderTransaction(this);
                yield return blockchain.wallet.TransactionEstimate(tx);

                if (tx.error.IsNotEmpty() && tx.error.Contains("insufficient allowance")) // Just not enouth money - not an error
                    callback?.Invoke(true);
                else
                    callback?.Invoke(tx.gasEstimation != default);
            }
#endif
            public void Cache()
            {
                blockchain = blockchainName.ToBlockchain();
                Log.Info("Cache - " + nftTokenName);
                var nftTokenView = BlockchainManager.GetTokenView(nftTokenName) as ERC1155View;
                nftToken = nftTokenView.Token;
                nft = nftTokenView.Items[nftId];
                nftTokenAddress = nftToken.GetAddress(blockchain);

                coinToken = BlockchainManager.GetToken(coinTokenName);
                coinTokenAddress = coinToken.GetAddress(blockchain);
                coinFeeAmount = coinAmount / (100 - nftToken.OwnerFee) * nftToken.OwnerFee;
                coinPrice = coinToken.FromMin(coinAmount / (100 - nftToken.OwnerFee) * 100);
                coinPricePerNFT = coinPrice / nftAmount;
                usdPricePerNFT = coinToken.UsdRate * coinPricePerNFT;
            }
        }

        public static BlockchainAddressDictionary Addresses;

        private const string ZONE = "0x54536b38F7644d1F34F3Ec5d172Db932F6783eb5";
        private const string CONDUIT_KEY = "0x0000000000000000000000000000000000000000000000000000000000000000";

        private static Dictionary<Blockchain, SmartContract> _contracts;

        static Seaport()
        {
            const string BASE_ADDRESS = "0x00000000006c3852cbEf3e08E8dF289169EdE581";

            Addresses = new BlockchainAddressDictionary
            {
                [Blockchain.Names.Ethereum] = BASE_ADDRESS,
                [Blockchain.Names.Polygon] = BASE_ADDRESS,
                [Blockchain.Names.BSC] = BASE_ADDRESS,
                [Blockchain.Names.NeoX] = Configurator.production ? "0x8832c58D42723D6A358aC21D5A4A5270F108523d" : "0x719d406Ed3873785f10F446D7aCD9dE5BdF3De3d",
            };

            _contracts = Addresses.ToContracts(nameof(Seaport));
        }

        public static IEnumerator CreateOrderEnumerator(Order.Type type, Blockchain blockchain, string offerer, Token nft, long nftId, long nftAmount,
            Token coin, decimal coinPrice, int coinFeePercentage, string coinFeeBeneficiary, Action<Order> callback)
        {
            var order = new Order
            {
                blockchainName = blockchain.name,

                type = type,
                Offerer = offerer,

                startTime = DateTime.UtcNow.ToTimestamp() / 1000,
                endTime = DateTime.UtcNow.AddYears(10).ToTimestamp() / 1000,

                nftTokenName = nft.GetContract(blockchain).Name.ToUpper(),
                nftId = nftId,
                nftAmount = nftAmount,

                coinTokenName = coin.GetContract(blockchain).Name.ToUpper(),
                coinAmount = (coin.ToMin(coinPrice) * (100 - nft.OwnerFee) / 100).ToLong(),

                salt = UnityEngine.Random.Range(100000000, 999999999),
            };
            order.Cache();

            Result<long> getCounterResult = null;
            yield return _contracts[order.blockchain].Functions["getCounter"].Read<long>(
               new DictSO { ["offerer"] = order.Offerer },
               result => getCounterResult = result);

            if (!getCounterResult.Success)
            {
                callback?.Invoke(null);
                yield break;
            }

            order.counter = getCounterResult.Value;

#if SG_BLOCKCHAIN
            var typedData = NethereumManager.GetTypedData(blockchain.network,
                message: new OrderComponents(order),
                new Type[] { typeof(OrderComponents), typeof(OrderComponents.OfferItem), typeof(OrderComponents.ConsiderationItem) },
                domainName: nameof(Seaport), domainVersion: "1.1", verifyingContract: Addresses[blockchain.name]);

            yield return blockchain.wallet.MessageSignEIP712(blockchain, typedData,
                result =>
                {
                    if (result.success)
                        order.signature = result.data["signature"].ToString();
                });
#endif

            callback?.Invoke(order);
        }

#if SG_BLOCKCHAIN
        public static IEnumerator FulfillBasicOrderEnumerator(Order order, Action<Transaction> callback)
        {
            yield return order.blockchain.wallet.TransactionSignAndSend(
                new FulfillBasicOrderTransaction(order), callback);
        }

        private static byte[] ToHexByte(this string value) =>
            Nethereum.Hex.HexConvertors.Extensions.HexByteConvertorExtensions.HexToByteArray(value);

        public class FulfillBasicOrderTransaction : Transaction
        {
            public const string TYPE = "MARKET_ORDER_FULFILL";
            public const string FUNCTION_NAME = "fulfillBasicOrder";

            public static SmartContract.Function GetFunction(Blockchain blockchain) =>
                _contracts[blockchain].Functions[FUNCTION_NAME];

            public FulfillBasicOrderTransaction(Order order) :
                base(GetFunction(order.blockchain), TYPE)
            {
                FunctionMessage = new Nethereum.Contracts.TransactionHandlers.Function<FulfillBasicOrderFunction>(
                    new FulfillBasicOrderFunction() { Parameters = new FulfillBasicOrderFunction.BasicOrderParameters(order) });

                Payload = new DictSO
                {
                    ["orderId"] = order.id,
                    ["orderType"] = order.type.ToString(),
                    ["tokenName"] = order.nft.Token.Name,
                };
            }

            [Function("fulfillBasicOrder", "bool")]
            private class FulfillBasicOrderFunction : Nethereum.Contracts.FunctionMessage
            {
                [Parameter("tuple", "parameters", 1)]
                public BasicOrderParameters Parameters { get; set; }

                public class BasicOrderParameters
                {
                    [Parameter("address", "considerationToken", 1)]
                    public string ConsiderationToken { get; set; } // contract address

                    [Parameter("uint256", "considerationIdentifier", 2)]
                    public BigInteger ConsiderationIdentifier { get; set; }

                    [Parameter("uint256", "considerationAmount", 3)]
                    public BigInteger ConsiderationAmount { get; set; }

                    [Parameter("address", "offerer", 4)]
                    public string Offerer { get; set; }

                    [Parameter("address", "zone", 5)]
                    public string Zone { get; set; }

                    [Parameter("address", "offerToken", 6)]
                    public string OfferToken { get; set; } // contract address

                    [Parameter("uint256", "offerIdentifier", 7)]
                    public BigInteger OfferIdentifier { get; set; }

                    [Parameter("uint256", "offerAmount", 8)]
                    public BigInteger OfferAmount { get; set; }

                    [Parameter("uint8", "basicOrderType", 9)]
                    public int BasicOrderType { get; set; }
                    public enum Type
                    {
                        // 0: no partial fills, anyone can execute
                        ETH_TO_ERC721_FULL_OPEN,

                        // 1: partial fills supported, anyone can execute
                        ETH_TO_ERC721_PARTIAL_OPEN,

                        // 2: no partial fills, only offerer or zone can execute
                        ETH_TO_ERC721_FULL_RESTRICTED,

                        // 3: partial fills supported, only offerer or zone can execute
                        ETH_TO_ERC721_PARTIAL_RESTRICTED,

                        // 4: no partial fills, anyone can execute
                        ETH_TO_ERC1155_FULL_OPEN,

                        // 5: partial fills supported, anyone can execute
                        ETH_TO_ERC1155_PARTIAL_OPEN,

                        // 6: no partial fills, only offerer or zone can execute
                        ETH_TO_ERC1155_FULL_RESTRICTED,

                        // 7: partial fills supported, only offerer or zone can execute
                        ETH_TO_ERC1155_PARTIAL_RESTRICTED,

                        // 8: no partial fills, anyone can execute
                        ERC20_TO_ERC721_FULL_OPEN,

                        // 9: partial fills supported, anyone can execute
                        ERC20_TO_ERC721_PARTIAL_OPEN,

                        // 10: no partial fills, only offerer or zone can execute
                        ERC20_TO_ERC721_FULL_RESTRICTED,

                        // 11: partial fills supported, only offerer or zone can execute
                        ERC20_TO_ERC721_PARTIAL_RESTRICTED,

                        // 12: no partial fills, anyone can execute
                        ERC20_TO_ERC1155_FULL_OPEN,

                        // 13: partial fills supported, anyone can execute
                        ERC20_TO_ERC1155_PARTIAL_OPEN,

                        // 14: no partial fills, only offerer or zone can execute
                        ERC20_TO_ERC1155_FULL_RESTRICTED,

                        // 15: partial fills supported, only offerer or zone can execute
                        ERC20_TO_ERC1155_PARTIAL_RESTRICTED,

                        // 16: no partial fills, anyone can execute
                        ERC721_TO_ERC20_FULL_OPEN,

                        // 17: partial fills supported, anyone can execute
                        ERC721_TO_ERC20_PARTIAL_OPEN,

                        // 18: no partial fills, only offerer or zone can execute
                        ERC721_TO_ERC20_FULL_RESTRICTED,

                        // 19: partial fills supported, only offerer or zone can execute
                        ERC721_TO_ERC20_PARTIAL_RESTRICTED,

                        // 20: no partial fills, anyone can execute
                        ERC1155_TO_ERC20_FULL_OPEN,

                        // 21: partial fills supported, anyone can execute
                        ERC1155_TO_ERC20_PARTIAL_OPEN,

                        // 22: no partial fills, only offerer or zone can execute
                        ERC1155_TO_ERC20_FULL_RESTRICTED,

                        // 23: partial fills supported, only offerer or zone can execute
                        ERC1155_TO_ERC20_PARTIAL_RESTRICTED
                    }

                    [Parameter("uint256", "startTime", 10)]
                    public BigInteger StartTime { get; set; }

                    [Parameter("uint256", "endTime", 11)]
                    public BigInteger EndTime { get; set; }

                    [Parameter("bytes32", "zoneHash", 12)]
                    public byte[] ZoneHash { get; set; }

                    [Parameter("uint256", "salt", 13)]
                    public BigInteger Salt { get; set; }

                    [Parameter("bytes32", "offererConduitKey", 14)]
                    public byte[] OffererConduitKey { get; set; }

                    [Parameter("bytes32", "fulfillerConduitKey", 15)]
                    public byte[] FulfillerConduitKey { get; set; }

                    [Parameter("uint256", "totalOriginalAdditionalRecipients", 16)]
                    public BigInteger TotalOriginalAdditionalRecipients { get; set; }

                    [Parameter("tuple[]", "additionalRecipients", 17)]
                    public AdditionalRecipient[] AdditionalRecipients { get; set; }
                    public class AdditionalRecipient
                    {
                        [Parameter("uint256", "amount", 1)]
                        public BigInteger Amount { get; set; }

                        [Parameter("address", "recipient", 2)]
                        public string Recipient { get; set; }
                    }

                    [Parameter("bytes", "signature", 18)]
                    public byte[] Signature { get; set; }

                    public BasicOrderParameters(Order order)
                    {
                        StartTime = order.startTime;
                        EndTime = order.endTime;

                        Zone = ZONE;
                        Salt = order.salt;
                        ZoneHash = order.counter.ToString().ToHexByte();

                        OffererConduitKey = CONDUIT_KEY.ToHexByte();
                        FulfillerConduitKey = CONDUIT_KEY.ToHexByte();

                        Signature = order.signature.ToHexByte();

                        AdditionalRecipients = new AdditionalRecipient[]
                        {
                            new AdditionalRecipient { Amount = order.coinFeeAmount, Recipient = order.nftToken.OwnerAddress }
                        };
                        TotalOriginalAdditionalRecipients = AdditionalRecipients.Length;

                        if (order.IsListing())
                        {
                            Offerer = order.seller;
                            BasicOrderType = (int) Type.ERC20_TO_ERC1155_FULL_OPEN;

                            OfferToken = order.nftTokenAddress;
                            OfferIdentifier = order.nftId;
                            OfferAmount = order.nftAmount;

                            ConsiderationToken = order.coinTokenAddress;
                            ConsiderationIdentifier = 0;
                            ConsiderationAmount = order.coinAmount;
                        }
                        else
                        {
                            Offerer = order.buyer;
                            BasicOrderType = (int) Type.ERC1155_TO_ERC20_FULL_OPEN;

                            OfferToken = order.coinTokenAddress;
                            OfferIdentifier = 0;
                            OfferAmount = order.coinAmount + order.coinFeeAmount; // Why is that? Cause fuck you!

                            ConsiderationToken = order.nftTokenAddress;
                            ConsiderationIdentifier = order.nftId;
                            ConsiderationAmount = order.nftAmount;
                        }
                    }
                }
            }
        }

        [Struct("OrderComponents")]
        public class OrderComponents
        {
            [Parameter("address", "offerer", 1)]
            public string offerer { get; set; }

            [Parameter("address", "zone", 2)]
            public string zone { get; set; }

            [Parameter("tuple[]", "offer", 3, "OfferItem[]")]
            public OfferItem[] offer { get; set; }
            [Struct("OfferItem")]
            public class OfferItem
            {
                [Parameter("uint8", "itemType", 1)]
                public int itemType { get; set; }

                [Parameter("address", "token", 2)]
                public string token { get; set; } // contract address

                [Parameter("uint256", "identifierOrCriteria", 3)]
                public BigInteger identifierOrCriteria { get; set; }

                [Parameter("uint256", "startAmount", 4)]
                public BigInteger startAmount { get; set; }

                [Parameter("uint256", "endAmount", 5)]
                public BigInteger endAmount { get; set; }
            }

            [Parameter("tuple[]", "consideration", 4, "ConsiderationItem[]")]
            public ConsiderationItem[] consideration { get; set; }
            [Struct("ConsiderationItem")]
            public class ConsiderationItem
            {
                [Parameter("uint8", "itemType", 1)]
                public int itemType { get; set; }

                [Parameter("address", "token", 2)]
                public string token { get; set; } // contract address

                [Parameter("uint256", "identifierOrCriteria", 3)]
                public BigInteger identifierOrCriteria { get; set; }

                [Parameter("uint256", "startAmount", 4)]
                public BigInteger startAmount { get; set; }

                [Parameter("uint256", "endAmount", 5)]
                public BigInteger endAmount { get; set; }

                [Parameter("address", "recipient", 6)]
                public string recipient { get; set; }
            }

            [Parameter("uint8", "orderType", 5)]
            public int orderType { get; set; }

            [Parameter("uint256", "startTime", 6)]
            public BigInteger startTime { get; set; }

            [Parameter("uint256", "endTime", 7)]
            public BigInteger endTime { get; set; }

            [Parameter("bytes32", "zoneHash", 8)]
            public byte[] zoneHash { get; set; }

            [Parameter("uint256", "salt", 9)]
            public BigInteger salt { get; set; }

            [Parameter("bytes32", "conduitKey", 10)]
            public byte[] conduitKey { get; set; }

            [Parameter("uint256", "counter", 11)]
            public BigInteger counter { get; set; }

            public enum ItemType
            {
                NATIVE = 0,
                ERC20 = 1,
                ERC721 = 2,
                ERC1155 = 3,
                ERC721_WITH_CRITERIA = 4,
                ERC1155_WITH_CRITERIA = 5,
            }

            public enum OrderType
            {
                FULL_OPEN = 0, // No partial fills, anyone can execute
                PARTIAL_OPEN = 1, // Partial fills supported, anyone can execute
                FULL_RESTRICTED = 2, // No partial fills, only offerer or zone can execute
                PARTIAL_RESTRICTED = 3, // Partial fills supported, only offerer or zone can execute
            }

            public OrderComponents(Order order)
            {
                zone = ZONE;
                orderType = (int) OrderType.FULL_OPEN;
                startTime = order.startTime;
                endTime = order.endTime;
                zoneHash = order.counter.ToString().ToHexByte();
                salt = order.salt;
                conduitKey = CONDUIT_KEY.ToHexByte();
                counter = order.counter;

                if (order.IsListing())
                {
                    offerer = order.seller;

                    offer = new OfferItem[]
                    {
                        new OfferItem
                        {
                            itemType = (int) ItemType.ERC1155,
                            token = order.nftTokenAddress,
                            identifierOrCriteria = order.nftId,
                            startAmount = order.nftAmount,
                            endAmount = order.nftAmount,
                        }
                    };

                    consideration = new ConsiderationItem[]
                    {
                        new ConsiderationItem
                        {
                            itemType = (int) ItemType.ERC20,
                            token = order.coinTokenAddress,
                            identifierOrCriteria = 0,
                            startAmount = order.coinAmount,
                            endAmount = order.coinAmount,
                            recipient = offerer,
                        },
                        new ConsiderationItem
                        {
                            itemType = (int) ItemType.ERC20,
                            token = order.coinTokenAddress,
                            identifierOrCriteria = 0,
                            startAmount =  order.coinFeeAmount,
                            endAmount = order.coinFeeAmount,
                            recipient = order.nftToken.OwnerAddress,
                        },
                    };
                }
                else
                {
                    offerer = order.buyer;

                    var offerItemAmount = order.coinAmount + order.coinFeeAmount; // Why is that? Cause fuck you!
                    offer = new OfferItem[]
                    {
                        new OfferItem
                        {
                            itemType = (int) ItemType.ERC20,
                            token = order.coinTokenAddress,
                            identifierOrCriteria = 0,
                            startAmount = offerItemAmount,
                            endAmount = offerItemAmount,
                        }
                    };

                    consideration = new ConsiderationItem[]
                    {
                        new ConsiderationItem
                        {
                            itemType = (int) ItemType.ERC1155,
                            token = order.nftTokenAddress,
                            identifierOrCriteria = order.nftId,
                            startAmount = order.nftAmount,
                            endAmount = order.nftAmount,
                            recipient = offerer,
                        },
                        new ConsiderationItem
                        {
                            itemType = (int) ItemType.ERC20,
                            token = order.coinTokenAddress,
                            identifierOrCriteria = 0,
                            startAmount =  order.coinFeeAmount,
                            endAmount = order.coinFeeAmount,
                            recipient = order.nftToken.OwnerAddress,
                        },
                    };
                }
            }
        }
#endif
    }
}