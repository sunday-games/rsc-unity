using UnityEngine;
using System;
using System.Collections.Generic;
using CodeStage.AntiCheat.ObscuredTypes;

public class Config : Core
{
    public string webServiceUrl = "https://script.google.com/macros/s/AKfycbzKa600Bd4wDCXzWrDDY0zZldcpzRiXm0QaHjClEaEb6tE2M_A/exec?ssid={0}&sheet={1}";
    public string spreadsheetId;

    Dictionary<string, object> data = null;

    public void Load()
    {
        var tableName = platform.ToString();
        if (build.premium) tableName += " Premium";
        tableName += " v2";

        Download.Create(gameObject).Run("Config", string.Format(webServiceUrl, spreadsheetId, tableName),
            download =>
            {
                if (!download.isSuccess)
                    return;

                var rows = Json.Deserialize(download.www.text) as List<object>;
                if (rows == null || rows.Count == 0)
                {
                    LogError("Config - Response parsing failed. Response: " + download.www.text);
                    return;
                }

                var firstRowDict = rows[0] as Dictionary<string, object>;
                if (firstRowDict == null)
                {
                    LogError("Config - Response parsing failed. Response: " + download.www.text);
                    return;
                }

                var hash = (string)firstRowDict["hash"];
                firstRowDict.Remove("hash");
                if (hash != (Json.Serialize(new List<object>() { firstRowDict }) + build.s).MD5())
                {
                    LogError("Config - Hash validation failed. Response: " + download.www.text);
                    return;
                }

                Log("Config - Loading Success. Json: " + download.www.text);
                data = firstRowDict;
                ObscuredPrefs.SetString("config", Json.Serialize(data));

                Setup();
            });
    }

    public void Setup()
    {
        if (data == null) data = Json.Deserialize(ObscuredPrefs.GetString("config", "{}")) as Dictionary<string, object>;

        if (data.ContainsKey("currentVersionCode")) build.currentVersionCode = Convert.ToInt32(data["currentVersionCode"]);
        if (data.ContainsKey("criticalVersionCode")) build.criticalVersionCode = Convert.ToInt32(data["criticalVersionCode"]);
        if (data.ContainsKey("updateUrl")) build.updateUrl = (string)data["updateUrl"];

        if (data.ContainsKey("facebook")) build.facebook = Convert.ToBoolean(data["facebook"]);

        if (data.ContainsKey("parentGate")) build.parentGate = Convert.ToBoolean(data["parentGate"]);

        if (data.ContainsKey("promocodes")) build.promocodes = Convert.ToBoolean(data["promocodes"]);

        if (data.ContainsKey("serverPurchaseVerification")) build.serverPurchaseVerification = Convert.ToBoolean(data["serverPurchaseVerification"]);

        if (data.ContainsKey("adMinLevel")) ads.minLevel = Convert.ToInt32(data["adMinLevel"]);
        if (data.ContainsKey("adInterstitialFrequency")) ads.interstitialFrequency = Convert.ToInt32(data["adInterstitialFrequency"]);
        ads.sessions = ads.interstitialFrequency / 2;

        // [10000,25000,50000,100000,250000,500000,750000,1000000,20000000]
        if (data.TryGetValue("leagues", out object leagues))
        {
            var leaguesList = Json.Deserialize((string)leagues) as List<object>;
            for (int i = 0; i < leaguesList.Count; ++i)
                gameplay.leagues[i].score = Convert.ToInt32(leaguesList[i]);
        }

        if (data.TryGetValue("eventSaleStartDate", out object eventSaleStartDate)) Events.sale.startDate = ToDate(eventSaleStartDate);
        if (data.TryGetValue("eventSaleEndDate", out object eventSaleEndDate)) Events.sale.endDate = ToDate(eventSaleEndDate);
        if (Events.sale.isActive && data.TryGetValue("eventSale", out object eventSale))
        {
            Events.sale.data = Json.Deserialize((string)eventSale) as List<object>;

            foreach (Dictionary<string, object> saleItem in Events.sale.data)
            {
                if (IAP.FromName((string)saleItem["iap"]) == iapManager.sausages)
                    ui.shop.xSausages = Convert.ToInt32(saleItem["multiplier"]);
                else if (IAP.FromName((string)saleItem["iap"]) == iapManager.goldfishes)
                    ui.shop.xGoldfishes = Convert.ToInt32(saleItem["multiplier"]);
            }
        }
        //Events.sale.TurnOn();

        if (data.TryGetValue("eventNYHStartDate", out object eventNYHStartDate)) Events.newYear.startDate = ToDate(eventNYHStartDate);
        if (data.TryGetValue("eventNYHEndDate", out object eventNYHEndDate)) Events.newYear.endDate = ToDate(eventNYHEndDate);
        //Events.newYear.TurnOff();

        if (data.TryGetValue("eventSVStartDate", out object eventSVStartDate)) Events.stValentin.startDate = ToDate(eventSVStartDate);
        if (data.TryGetValue("eventSVEndDate", out object eventSVEndDate)) Events.stValentin.endDate = ToDate(eventSVEndDate);
        //Events.stValentin.TurnOn();

        if (data.TryGetValue("eventHWStartDate", out object eventHWStartDate)) Events.halloween.startDate = ToDate(eventHWStartDate);
        if (data.TryGetValue("eventHWEndDate", out object eventHWEndDate)) Events.halloween.endDate = ToDate(eventHWEndDate);
        //Events.halloween.TurnOn();

        if (data.ContainsKey("landingPage")) server.links.landingPage = (string)data["landingPage"];

        if (data.ContainsKey("supportEmail")) server.links.supportEmail = (string)data["supportEmail"];
    }

    DateTime ToDate(object obj)
    {
        var date = obj.ToString().Split(new char[] { '-' });
        if (date.Length < 3) return default;

        var year = Convert.ToInt32(date[0]);
        var month = Convert.ToInt32(date[1]);
        var day = Convert.ToInt32(date[2].Length <= 2 ? date[2] : date[2].Substring(0, 2));

        return new DateTime(year, month, day);
    }
}
