using LYNC;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections.Generic;

public class ROUTES
{
    public readonly static string GENERIC_TRANSACTION = LyncManager.BaseServerURL + "transactions/send";
    public readonly static string KEYLESS_TRANSACTION = LyncManager.BaseServerURL + "/api/keyless/" + "transaction";
    public readonly static string BALANCE = LyncManager.BaseServerURL + "wallet/balance";
    public readonly static string PROFILE = LyncManager.BaseServerURL + "users/profile";
    public readonly static string MOBILE_TRANSACTION = LyncManager.BaseServerURL + "/api/unity/" + "mobile-transaction-builder";
}

public class API
{
    public static ROUTES ROUTES;

    public delegate void OnSuccess(ServerBasedTransactionFeedback tsxData);
    public delegate void OnError(string error);

    public static IEnumerator CoroutineTransaction(string url, Transaction customTransaction, System.Action<ServerBasedTransactionFeedback> onSuccess, System.Action<TransactionResult> onError)
    {
        UnityWebRequest webRequest = UnityWebRequest.Put(url, customTransaction.ToJson());
        webRequest.method = "POST";
        webRequest.SetRequestHeader("Content-Type", "application/json");
        webRequest.SetRequestHeader("x-api-key", LyncManager.Instance.xApiKey);
        yield return webRequest.SendWebRequest();

        if (webRequest.result == UnityWebRequest.Result.Success)
        {
            ServerBasedTransactionFeedback tsxData = JsonUtility.FromJson<ServerBasedTransactionFeedback>(webRequest.downloadHandler.text);
            onSuccess(tsxData);
        }
        else
        {
            TransactionResult tsxData = JsonUtility.FromJson<TransactionResult>(webRequest.downloadHandler.text);
            tsxData.success = false;
            onError(tsxData);
            Debug.Log(webRequest.error);
        }
    }
    public static IEnumerator CoroutineViewTransaction(ViewTransection customTransaction, System.Action<ViewTransectionResult> onSuccess, System.Action<TransactionResult> onError)
    {
        string url = LyncManager.BaseServerURL + "transactions/view";
        UnityWebRequest webRequest = UnityWebRequest.Put(url, JsonUtility.ToJson(customTransaction));
        webRequest.method = "POST";
        webRequest.SetRequestHeader("Content-Type", "application/json");
        webRequest.SetRequestHeader("x-api-key", LyncManager.Instance.xApiKey);
        yield return webRequest.SendWebRequest();

        if (webRequest.result == UnityWebRequest.Result.Success)
        {
            ViewTransectionResult tsxData = JsonUtility.FromJson<ViewTransectionResult>(webRequest.downloadHandler.text);
            Debug.Log(webRequest.downloadHandler.text);
            // Debug.Log(tsxData.ToString());
            onSuccess(tsxData);
        }
        else
        {
            TransactionResult tsxData = JsonUtility.FromJson<TransactionResult>(webRequest.downloadHandler.text);
            tsxData.success = false;
            onError(tsxData);
        }
    }
    public static IEnumerator CoroutineGetBalance(string WalletAddress, System.Action<float> onSuccess, System.Action<string> onError)
    {
        string url = $"{LyncManager.BaseServerURL}wallet/balance?network={(int)LyncManager.Instance.Network}&accountAddress={WalletAddress}";
        UnityWebRequest webRequest = UnityWebRequest.Get(url);       
        webRequest.SetRequestHeader("Content-Type", "application/json");
        webRequest.SetRequestHeader("x-api-key", LyncManager.Instance.xApiKey);
        // Debug.Log("Sending request " + url); 
        yield return webRequest.SendWebRequest();

        if (webRequest.result == UnityWebRequest.Result.Success)
        {
            // Debug.Log("webRequest.downloadHandler.text"+webRequest.downloadHandler.text);
            BalanceDataOutput balanceData = JsonUtility.FromJson<BalanceDataOutput>(webRequest.downloadHandler.text);
            string balance = balanceData.data.data;
            onSuccess(float.Parse(balance));
        }
        else
        {
            // Debug.Log(webRequest.downloadHandler.text);
            onError("Error when getting balance");
            Debug.LogError(webRequest.error);
        }
    }


    public static IEnumerator CoroutineCheckAPIKey(string uri, System.Action<bool> onSuccess, System.Action<string> onError)
    {
        UnityWebRequest webRequest = UnityWebRequest.Get(uri);
        // webRequest.method = "POST";
        webRequest.SetRequestHeader("Content-Type", "application/json");

        yield return webRequest.SendWebRequest();

        if (webRequest.result == UnityWebRequest.Result.Success)
        {
            // Debug.Log(webRequest.downloadHandler.text);
            APIKeyCheckData apiResult = JsonUtility.FromJson<APIKeyCheckData>(webRequest.downloadHandler.text);
            onSuccess(apiResult.status == 200);
        }
        else
        {
            // Debug.Log(webRequest.downloadHandler.text);
            onError(webRequest.error);
            Debug.LogError(webRequest.error);
        }
    }

    public static IEnumerator CoroutineGetFirebaseProfile(SupraProfileScheme aptosProfileData, System.Action<SupraFirebaseAuthDetails> onSuccess, System.Action<string> onError)
    {
        UnityWebRequest webRequest = UnityWebRequest.Put(ROUTES.PROFILE, JsonUtility.ToJson(aptosProfileData));
        // Debug.Log(JsonUtility.ToJson(aptosProfileData));
        webRequest.method = "POST";
        webRequest.SetRequestHeader("Content-Type", "application/json");
        webRequest.SetRequestHeader("x-api-key", LyncManager.Instance.xApiKey);


        // Debug.Log($"Sending web request GET profile email: [{aptosProfileData.email}] - firebaseUid: [{aptosProfileData.firebaseUid}]");
        yield return webRequest.SendWebRequest();

        if (webRequest.result == UnityWebRequest.Result.Success)
        {
            // Debug.Log(webRequest.downloadHandler.text);
            SupraFirebaseAuthDetails aptosResponse = JsonUtility.FromJson<SupraFirebaseAuthData>(webRequest.downloadHandler.text).data;
            onSuccess(aptosResponse);
        }
        else
        {
            onError(webRequest.error);
        }
    }

    public static IEnumerator CouroutineBuildMobileTransaction(Transaction transaction, System.Action<string> onSuccess, System.Action<string> onError)
    {
        UnityWebRequest webRequest = UnityWebRequest.Put(ROUTES.MOBILE_TRANSACTION, JsonUtility.ToJson(transaction));
        webRequest.method = "POST";
        webRequest.SetRequestHeader("Content-Type", "application/json");
        webRequest.SetRequestHeader("x-api-key", LyncManager.Instance.xApiKey);


        yield return webRequest.SendWebRequest();

        if (webRequest.result == UnityWebRequest.Result.Success)
        {
            // Debug.Log(webRequest.downloadHandler.text);
            onSuccess(webRequest.downloadHandler.text);
        }
        else
        {
            Debug.LogError(webRequest.error);
            onError(webRequest.error);
        }
    }

    public static IEnumerator CoroutineLoginSendAnalytics(string ApiKey, string walletAddress, string network, string loginMethod)
    {
        // Debug.Log("CoroutineLoginSendAnalytics");
        AnalyticsData jsonObject = new AnalyticsData
        {
            apiKey = ApiKey,
            walletAddress = walletAddress,
            network = network,
            loginMethod = loginMethod
        };

        var jsonData = JsonUtility.ToJson(jsonObject);
        string RequestURL = "https://server.lync.world/aptos-unity-sdk/user-login";
        using (UnityWebRequest www = UnityWebRequest.Put(RequestURL, jsonData))
        {
            www.method = "POST";
            www.SetRequestHeader("Content-Type", "application/json");
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                // Debug.Log("Invalid API Key: " + www.error);
            }
            else
            {
                //Debug.Log("www" + www);
            }
        }
    }

    public static IEnumerator CoroutineSendTransactionsAnalytics(string ApiKey, string walletAddress, string network, string txnHash, string paymentMode)
    {
        TransactionData jsonObject = new TransactionData
        {
            apiKey = ApiKey,
            walletAddress = walletAddress,
            network = network,
            txnHash = txnHash,
            paymentMode = paymentMode
        };

        var jsonData = JsonUtility.ToJson(jsonObject);

        // Debug.LogError("jsonData"+jsonData);
        // Debug.LogError("jsonObject"+ jsonObject);

        string RequestURL = "https://server.lync.world/aptos-unity-sdk/user-transactions";
        using (UnityWebRequest www = UnityWebRequest.Put(RequestURL, jsonData))
        {
            www.method = "POST";
            www.SetRequestHeader("Content-Type", "application/json");
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log("Invalid API Key: " + www.error);
            }
            else
            {
                Debug.Log("www" + www);
            }
        }
    }
}
