using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DictSO = System.Collections.Generic.Dictionary<string, object>;
using ListO = System.Collections.Generic.List<object>;

namespace SG.BlockchainPlugin
{
    [Serializable]
    public class NFT
    {
        public static implicit operator bool(NFT nft) => nft != null;

        private const string IFPS_PREFIX = "ipfs://";
        private const string IFPS_HTTPS_PREFIX = "https://gateway.ipfs.io/ipfs/";

        public string Key;
        public int TokenId = -1;
        public string ImageUrl;
        [NonSerialized] public Token Token;

        private Vector2 _pivot = new Vector2(0.5f, 0.22f);

        [NonSerialized] private Sprite _imageSprite;
        [NonSerialized] private NFTMetadata _metadata;

        public string Name => _metadata != null && _metadata.Name.IsNotEmpty() ? _metadata.Name : Key;
        public string Descriprion;
        public DictSO Properties => _metadata != null && _metadata.Properties != null ? _metadata.Properties : new DictSO();

        public bool HasToken => TokenId >= 0;

        public void GetImageSprite(Action<Sprite> callback)
        {
            if (_imageSprite != null)
                callback?.Invoke(_imageSprite);
            else if (ImageUrl.IsEmpty())
                callback?.Invoke(null);
            else
                new Download(ImageUrl, type: Download.Type.GET_IMAGE)
                    .SetCallback(download =>
                    {
                        _imageSprite = download.GetCryptoAssetSprite(_pivot);
                        callback?.Invoke(_imageSprite);
                    })
                    .Run();
        }

        public IEnumerator GetImageSpriteCoroutine(Action<Sprite> callback)
        {
            if (_imageSprite != null)
                callback?.Invoke(_imageSprite);
            else if (ImageUrl.IsEmpty())
                callback?.Invoke(null);
            else
                yield return new Download(ImageUrl, type: Download.Type.GET_IMAGE)
                    .SetCallback(download =>
                    {
                        _imageSprite = download.GetCryptoAssetSprite(_pivot);
                        callback?.Invoke(_imageSprite);
                    })
                    .RequestCoroutine();
        }

        public void GetMetadata(Action<NFTMetadata> callback)
        {
            if (_metadata != null) // If metadata is already loaded
            {
                callback?.Invoke(_metadata);
                return;
            }

            if (Token == null || Token.MetaUrl.IsEmpty())
            {
                callback?.Invoke(null);
                return;
            }

            var url = Token.MetaUrl.Contains(IFPS_PREFIX) ?
                Token.MetaUrl.Replace(IFPS_PREFIX, IFPS_HTTPS_PREFIX) :
                Token.MetaUrl;

            url = string.Format(url, TokenId);

            new Download(url)
                .SetCallback(download =>
                {
                    if (download.success && download.responseDict != null)
                        _metadata = new NFTMetadata(download.responseDict);

                    callback?.Invoke(_metadata);
                    return;
                })
                .IgnoreError()
                .Run();
        }

        public void OpenOpenseaUrl(Blockchain blockchain)
        {
            if (!HasToken)
                return;

            var blockchainName = blockchain == Blockchain.polygon ? "matic" : blockchain.name.ToString().ToLower();

            UI.Helpers.OpenLink($"https://opensea.io/assets/{blockchainName}/{Token.GetAddress(blockchain)}/{TokenId}");
        }

        public bool TotalSupply(Blockchain blockchain, out int totalSupply)
        {
            if (Token.TotalSupplyAmounts.ContainsKey(TokenId) && Token.TotalSupplyAmounts[TokenId].ContainsKey(blockchain))
            {
                totalSupply = Token.TotalSupplyAmounts[TokenId][blockchain].ToInt();
                return true;
            }

            totalSupply = default;
            return false;
        }
    }
}