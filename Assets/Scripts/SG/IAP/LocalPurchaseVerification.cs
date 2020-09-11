#if UNITY_ANDROID || UNITY_IPHONE || UNITY_STANDALONE_OSX || UNITY_TVOS

using System;
using UnityEngine;
using UnityEngine.Purchasing.Security;

namespace SG
{
    public static class LocalPurchaseVerification
    {
        static CrossPlatformValidator validator;

        class VerificationData
        {
            public byte[] data;
            public int[] order;
            public int key;

            public byte[] DeObfuscate() { return Obfuscator.DeObfuscate(data, order, key); }
        }

        static LocalPurchaseVerification()
        {
            VerificationData apple, google;

            if (Core.isTNT)
            {
                apple = new VerificationData()
                {
                    data = Convert.FromBase64String("24qcqvSq5qIMK0hJIyFw7wV+5+/t2tPW3tHc2p/Q0Z/L19bMn9zazYkm85LHCFIzJGNMyCRNyW3Ij/B+oDo8OqQmgviITRYk/zGTaw4vrWfR25/c0NHb1svW0NHMn9DZn8rM2uYYurbDqP/prqHLbAg0nIT4HGrQPb6/ubaVOfc5SNzbur6PPk2PlbmVOfc5SLK+vrq6v4/djrSPtrm86rfhjz2+rrm86qKfuz2+t489vruPuY+wubzqoqy+vkC7uo+8vr5Aj6KQjz58ubeUub66uri9vY8+CaU+DAFLzCRRbduwdMbwi2cdgUbHQNR3MMw+33mk5LaQLQ1H+/dP34chqkqPrrm86ru1rLX+z8/T2p/20dyRjp/8/o89vp2Psrm2lTn3OUiyvr6+zd7cy9bc2p/My97L2tLa0cvMkY/T2p/20dyRjpmPm7m86ru0rKL+z8/T2p/82s3L1tnW3N7L1tDRn/7Kxp/ezMzK0trMn97c3NrPy97R3Nr2Z8kgjKvaHsgrdpK9vL6/vhw9voqNjouPjInlqLKMio+Nj4aNjouPt5S5vrq6uL2+qaHXy8vPzIWQkMjFjz2+yY+xubzqorC+vkC7u7y9vrq/vD2+sL+PPb61vT2+vr9bLha2jz27BI89vBwfvL2+vb2+vY+yubaMieWP3Y60j7a5vOq7uay96uyOrBQczi347Op+EJD+DEdEXM9yWRzz3dPan8zL3tHb3s3bn8vazdLMn96Tn9zazcvW2dbc3svan8/Q09bcxnamzUrisWrA4CRNmrwF6jDy4rJOoC5kofjvVLpS4cY7klSJHejz6lPW2dbc3svW0NGf/srL19DN1svGjpmPm7m86ru0rKL+z8/T2p/82s3Lu7msversjqyPrrm86ru1rLX+z880pjZhRvTTSrgUnY+9V6eBR++2bAqFEkuwsb8ttA6eqZHLaoOyZN2pn9DZn8vX2p/L19rRn97Pz9PW3N6pj6u5vOq7vKyy/s/P09qf7dDQy7K5tpU59zlIsr6+urq/vD2+vr/jgpnYnzWM1UiyPXBhVByQRuzV5NsqIcWzG/g05GupiIx0e7DycavWbptdVG4Iz2Cw+l6YdU7Sx1JYCqiowP4XJ0Zuddkjm9SubxwEW6SVfKDL19DN1svGjqmPq7m86ru8rLL+z7m86qKxu6m7q5Rv1vgrybZBS9Qy2DC3C59IdBOTn9DPCYC+jzMI/HCwIoJMlPaXpXdBcQoGsWbho2l0gsjIkd7Pz9PakdzQ0pDez8/T2tzeuFPChjw07J9sh3sOACXwtdRAlEOR/xlI+PLAt+GPoLm86qKcu6ePqcvW2dbc3svan93Gn97Rxp/P3s3LZonAfjjqZhgmBo39RGdqziHBHu36waDz1O8p/jZ7y920rzz+OIw1Pj+rlG/W+CvJtkFL1DKR/xlI+PLADo/nU+W7jTPXDDCiYdrMQNjh2gMIpAIs/ZutlXiwognyI+Hcd/Q/qH/cjMhIhbiT6VRlsJ6xZQXMpvAKn97R25/c2s3L1tnW3N7L1tDRn8/P09qf7dDQy5/8/o+hqLKPiY+LjRdjwZ2KdZpqZrBp1Gsdm5yuSB4T7xU1amVbQ2+2uIgPysqe"),
                    order = new int[] { 20, 38, 48, 49, 43, 52, 34, 38, 33, 29, 37, 50, 55, 29, 29, 46, 49, 43, 40, 27, 38, 35, 28, 29, 38, 33, 38, 42, 44, 42, 46, 50, 37, 48, 49, 42, 41, 55, 47, 56, 40, 53, 43, 52, 46, 52, 46, 53, 51, 50, 56, 58, 58, 55, 55, 59, 57, 58, 58, 59, 60 },
                    key = 191,
                };

                google = new VerificationData()
                {
                    // Google Play public key: MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEApYVJg8zM05pRbUlVojlxrTYtuVEhnBR4Yazxcs6vkyDNC98JpP8gIZoPtvXAD1IpaqTBz2xFq29hee/Mqi2H8cuF9IgmVMLHtwxXa0sg7CXw3oHvW+aXY5+yv4BiC5DdoLbNsqj8OiFhMF+XNICbVBXP64I0v18zbYOJyvM/0pNV0iETuTOO8N8aMXRlZ/EU/YCcrXS/O3Rj8ax9z/lR0npMN+CjZVRJw3F4gT98DMPwfM5R//tZIbSEB55TDzNR4xiZEFxafwuGntRlVWVJqviLACRR3boUuR65jMIZEe++59Z8BjhnM6IOJsACeweN4I2qlMHSwCj2rdpXIxnhUQIDAQAB
                    data = Convert.FromBase64String("3Fbk7RSq6ZlWZelbxGpuzLQhEZIPxPjcwDes5DijuCzEtAmB7fQ5ZBeUmpWlF5SflxeUlJUwENwWWVlGOOEqruH2ZDnoWmzER+/ZonU28ME/bR6VscRIL4EsiywZV4yEeityQ6UXlLelmJOcvxPdE2KYlJSUkJWW9gonKhX3ngVINSNYJz1pr7T0pcods8FXUiKZwv7etXmwZUsUes5zAgbAR7SGLKYbZUqPpOHw8mSBaBUJC8aapsR2jQyFyc/qnhMLQfDA8NznWzoGtVieSpwxarW0D5ojYFWaxwKhFQ7BgFp+F6Eqyqb4FhxfZqpH6ZOt8qY3m7NVl+6SGHUYPwFUR1W8/zFUWvnQPvr07HpZP7gSZF4QYb1jOE/Ctox0xJeWlJWU"),
                    order = new int[] { 10,2,2,9,12,10,6,10,8,11,11,11,13,13,14 },
                    key = 149,
                };
            }
            else
            {
                apple = new VerificationData()
                {
                    data = Convert.FromBase64String("DgXhlz/cEMBPjayoUF+U1lWP8krhqxma7auVnZjOhpSammSfn5iZmiqrw3fBn6kX8ygUhkX+6GT8xf4nk7Cdmp6enJmajYXz7+/r6KG0tOy/eXBKLOtElN56vFFq9uN2fC6MjIQeGB6AAqbcrGkyANsVt08qC4lD7Oy1+uvr9/61+PT2tPrr6/f++Pquqaqvq6itwYyWqK6rqauiqaqvqxCCEkVi0PdunDC5q5lzg6Vjy5JIM0flua5Rvk5ClE3wTzm/uIpsOjf/rriO0I7ChigPbG0HBVTLIVrDy+K7+ujo7vb+6Lv6+Pj+6+/69fj+0kPtBKiP/jrsD1K2mZiam5o4GZqorcGr+aqQq5KdmM6fnYiZzsiqiPwUky+7bFA3t7v06y2kmqsXLNhU7/P06fLv4qqNq4+dmM6fmIiW2usU6Br7XYDAkrQJKWPf02v7owWObre7+P7p7/L98vj67/676/T38vji6/f+u8n09O+72NqrhYyWq62rr6kZmpudkrEd0x1s+P+emqsaaauxnZx35qIYEMi7SKNfKiQB1JHwZLBnMDjqCdzIzlo0tNooY2B461Z9ONe7+vX/u/j+6e/y/fL4+u/y9PW7652rlJ2YzoaImppkn56rmJqaZKuGsR3THWyWmpqenpur+aqQq5KdmM7CPJ6S54zbzYqF70gsELig3DhO9JadkrEd0x1slpqanp6bmBmampvH9/670vX4taq9q7+dmM6fkIiG2uuTxasZmoqdmM6Gu58ZmpOrGZqfq0Kt5FoczkI8AiKp2WBDTuoF5TrJu9jaqxmauauWnZKxHdMdbJaamprk2jMDYkpR/Qe/8IpLOCB/gLFYhOn6+O/y+P676O/67/72/vXv6LWrq4qdmM6fkYiR2uvr9/670vX4tarr9/672P7p7/L98vj67/L09bva7p2YzoaVn42fj7BL8twP7ZJlb/AW8v3y+Prv8vT1u9ru7/P06fLv4qqfnYiZzsiqiKuKnZjOn5GIkdrr6/X/u/j09f/y7/L09ei79P277uj+npuYGZqUm6sZmpGZGZqam38KMpIsgCYI2b+JsVyUhi3WB8X4U9AbjC6hNm+UlZsJkCq6jbXvTqeWQPmNyf738vr1+P679PW77/Py6Lv4/unv8v3y+Prv/rv54rv69eK76/rp760C17bjLHYXAEdo7ABp7Unsq9Ratds9bNzW5JPFq4SdmM6GuJ+Dq42ECkCF3MtwnnbF4h+2cK05zNfOd1KC6W7GlU7kxABpvpghzhTWxpZq3uWE1/DLDdoSX+/5kIsY2hyoERobj7BL8twP7ZJlb/AWtds9bNzW5L2rv52Yzp+QiIba6+v3/rvY/unv+ff+u+jv+vX/+un/u+/+6fbou/q79P277/P+u+/z/vW7+uvr9/L4+pQGpmiw0rOBU2VVLiKVQsWHTVCmtKsaWJ2TsJ2anp6cmZmrGi2BGiimvfy7EajxbJYZVEVwOLRiyPHA/6sZnyCrGZg4O5iZmpmZmpmrlp2SW/io7GyhnLfNcEGUupVBIeiC1C4lb+gAdUn/lFDi1K9DOaVi42TwU42rj52Yzp+YiJba6+v3/rvJ9PTvyzERTkF/Z0uSnKwr7u66"),
                    order = new int[] { 18, 28, 26, 35, 53, 49, 36, 7, 16, 54, 20, 40, 27, 16, 21, 49, 55, 44, 36, 34, 51, 24, 45, 29, 28, 52, 47, 35, 33, 58, 55, 44, 46, 54, 53, 55, 40, 47, 43, 54, 59, 58, 43, 54, 48, 48, 49, 53, 52, 49, 51, 54, 58, 58, 58, 56, 59, 57, 58, 59, 60 },
                    key = 155,
                };

                google = new VerificationData()
                {
                    // Google Play public key: MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEA0WWoOomW7wNujY1FWgcOtv1FaSLwN9xk/6t/8hFqX3LJNqIiRirET2ANLDj/NWMiJ68e+KJL6rqqJ0LXZAFvmluNiZYJZg3bpQaVafXrcUmYwB2PwiE8sLqv5t88UPQddYZ/Hmuwqto038JJsHZUxw3hGBd5k93DfOiZbZbbPv/XVpBB0pKVni8yb1Sdq1jmoAJz5sd9owOXYrWSyN9vo+OUuGGQ9OyAABHT1k3FFmm72h4g+lK2LvO+QLKEreQaPvxhI9RguXpdruMSqswb4Ny1OoH1sPBToxNLmu7rFhQZIUKjljPR4K+ue6My+qnJ9JoWYEvWkLK66pRsWVtoHwIDAQAB
                    data = Convert.FromBase64String("xnT31Mb78P/ccL5wAfv39/fz9vUQMYtV9WGUQ2Q+KZlVFWJOl2YCGhYqQ8x3A0YGpVXlvWwYHeDi79e0RHJbEuzICpfVIpZPjKtYFeRcOu31mHt7s6zx+EALs5/UBsEqkgldiVVgxScWWViNVcQMXz8CbOCWvSBmdvbnJSC7M+CfTSzo1gykQNgFSLZg/5D7LVPwY58DHYe/bjbreTTXygkhoGa3JGRjaNnEmaJrXa4QVvSF1NFZ6A5UvRxMXNG0IZL3mWyte3909/n2xnT3/PR09/f2J5NezH9gGUZMWRApyqYC64NwieidRlwswik0v0aAojH7F+7hj2UrNYoeb5tgLcgE55yphD/AVNSw3DK5lvvazgnDlURMHGKar62e6fT19/b3"),
                    order = new int[] { 0, 9, 12, 11, 12, 13, 10, 13, 8, 12, 12, 12, 13, 13, 14 },
                    key = 246,
                };
            }

            validator = new CrossPlatformValidator(google.DeObfuscate(), apple.DeObfuscate(), Application.identifier);
        }

        public static void Validate(string receipt, PurchaseData data, Action<bool> callback)
        {
            try
            {
                var result = validator.Validate(receipt);

                foreach (IPurchaseReceipt productReceipt in result)
                {
                    if (productReceipt.productID == data.iap.sku)
                    {
                        Core.Log("IAP - Purchase Verify Success");
                        callback?.Invoke(true);
                    }
                    else
                    {
                        Core.Log($"IAP - Purchase Failed. productID {productReceipt.productID}, purchaseDate {productReceipt.purchaseDate}, transactionID {productReceipt.transactionID}");

                        var google = productReceipt as GooglePlayReceipt;
                        if (google != null)
                        {
                            Core.LogDebug("IAP - purchaseState " + google.purchaseState);
                            Core.LogDebug("IAP - purchaseToken " + google.purchaseToken);
                        }

                        var apple = productReceipt as AppleInAppPurchaseReceipt;
                        if (apple != null)
                        {
                            Core.LogDebug("IAP - originalTransactionIdentifier " + apple.originalTransactionIdentifier);
                            Core.LogDebug("IAP - cancellationDate " + apple.cancellationDate);
                            Core.LogDebug("IAP - quantity " + apple.quantity);
                        }

                        callback?.Invoke(false);
                    }
                }
            }
            catch (IAPSecurityException)
            {
                Core.Log("IAP - Purchase Failed: Invalid receipt");
                callback?.Invoke(false);
            }
        }

        // Старый метод, хз работает ли
        //public override void Verify(PurchaseData data, Action<bool> callback)
        //{
        //    if (platform == Platform.AppStore || platform == Platform.tvOS)
        //    {
        //        var url = isDebug ? "https://sandbox.itunes.apple.com/verifyReceipt" : "https://buy.itunes.apple.com/verifyReceipt";
        //        var request = System.Text.Encoding.UTF8.GetBytes("{\"receipt-data\":\"" + data.receipt + "\"}");
        //        Download.Create(server.gameObject).Run("IAP - Apple", new WWW(url, request), download =>
        //        {
        //            if (download.isSuccess)
        //            {
        //                LogDebug("IAP - Apple - Server response: " + download.www.text);
        //                // {"receipt":{"original_purchase_date_pst":"2015-11-21 02:33:11 America/Los_Angeles", "purchase_date_ms":"1448101991792", "unique_identifier":"d8ad32d9d96612840fb64effe6ad45d5e6481cd6", "original_transaction_id":"1000000181287068", "bvrs":"1.3.5", "transaction_id":"1000000181287068", "quantity":"1", "unique_vendor_identifier":"B506F928-29A0-4D86-A5B4-F8B47B7FE02E", "item_id":"924907289", "product_id":"com.sgmw.rsc.luckywheel", "purchase_date":"2015-11-21 10:33:11 Etc/GMT", "original_purchase_date":"2015-11-21 10:33:11 Etc/GMT", "purchase_date_pst":"2015-11-21 02:33:11 America/Los_Angeles", "bid":"com.suchgamesmuchwow.readysetcat", "original_purchase_date_ms":"1448101991792"}, "status":0}

        //                var response = Json.Deserialize(download.www.text) as Dictionary<string, object>;

        //                if (response != null && response.ContainsKey("status") && Convert.ToInt32(response["status"]) == 0 &&
        //                    (string)((Dictionary<string, object>)response["receipt"])["product_id"] == data.iap.sku)
        //                {
        //                    Log("IAP - Apple - Verifyed");
        //                    callback(true);
        //                }
        //                else
        //                {
        //                    LogError("IAP - Apple - Failure to verify. Product ID: {0}, Apple Response: {1}", data.iap.sku, download.www.text);
        //                    callback(false);
        //                }
        //            }
        //            else callback(false);
        //        });
        //    }
        //    else if (platform == Platform.GooglePlay)
        //    {
        //        try
        //        {
        //            var receiptBytes = System.Text.Encoding.ASCII.GetBytes(data.receipt);
        //            var signBytes = Convert.FromBase64String((data.signature).Replace("\\", ""));

        //            using (var rsa = PEMKeyLoader.CryptoServiceProviderFromPublicKeyInfo(build.googlePublicKey))
        //            {
        //                if (rsa.VerifyData(receiptBytes, "SHA1", signBytes))
        //                {
        //                    Log("IAP - Google - Verify Succeed");
        //                    callback(true);
        //                }
        //                else
        //                {
        //                    LogError("IAP - Google - Verify Failure");
        //                    callback(false);
        //                }
        //            }
        //        }
        //        catch (Exception e)
        //        {
        //            LogError("IAP - Google - Verify Exception: " + e.StackTrace);
        //            callback(false);
        //        }
        //    }
        //}
    }
}
#endif
