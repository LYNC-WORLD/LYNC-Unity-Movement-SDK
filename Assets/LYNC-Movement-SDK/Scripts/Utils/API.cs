using LYNC;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections.Generic;
using LYNC.Wallet;

public class ROUTES
{
    public readonly static string GENERIC_TRANSACTION = LyncManager.BaseServerURL + "/api/transaction/generic";
    public readonly static string VIEW_TRANSACTION = LyncManager.BaseServerURL + "/api/transaction/view";
    public readonly static string BALANCE = LyncManager.BaseServerURL + "/api/unity/balance";
    public readonly static string PROFILE = LyncManager.BaseServerURL + "/api/user/profile";
    public readonly static string MOBILE_TRANSACTION = LyncManager.BaseServerURL + "/api/unity/" + "mobile-transaction-builder";
}

public class API
{
    public static ROUTES ROUTES;

    public delegate void OnSuccess(ServerBasedTransactionFeedback tsxData);
    public delegate void OnError(string error);

    public static IEnumerator CoroutineTransaction(Transaction customTransaction, System.Action<ServerBasedTransactionFeedback> onSuccess, System.Action<TransactionResult> onError)
    {
        string url = ROUTES.GENERIC_TRANSACTION;
        customTransaction.network = (int)LyncManager.Instance.Network;
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
            Debug.Log(webRequest.downloadHandler.text);
            TransactionResult tsxData = JsonUtility.FromJson<TransactionResult>(webRequest.downloadHandler.text);
            tsxData.success = false;
            onError(tsxData);
            Debug.Log(webRequest.error);
        }
    }

    public static IEnumerator CoroutineViewTransaction(ViewTransaction customTransaction, System.Action<string> onSuccess, System.Action<TransactionResult> onError)
    {
        string url = ROUTES.VIEW_TRANSACTION;

        customTransaction.network = (int)LyncManager.Instance.Network;
        UnityWebRequest webRequest = UnityWebRequest.Put(url, JsonUtility.ToJson(customTransaction));
        webRequest.method = "POST";
        webRequest.SetRequestHeader("Content-Type", "application/json");
        webRequest.SetRequestHeader("x-api-key", LyncManager.Instance.xApiKey);
        yield return webRequest.SendWebRequest();

        if (webRequest.result == UnityWebRequest.Result.Success)
        {
            onSuccess(webRequest.downloadHandler.text);
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
        string url = ROUTES.BALANCE;
        BalanceData balanceData = new BalanceData();
        balanceData.network = (int)LyncManager.Instance.Network;
        balanceData.publicKey = WalletAddress;

        string jsonData = JsonUtility.ToJson(balanceData);
        UnityWebRequest webRequest = UnityWebRequest.Put(url, jsonData);   
        webRequest.method = "POST";
        webRequest.SetRequestHeader("Content-Type", "application/json");
        webRequest.SetRequestHeader("x-api-key", LyncManager.Instance.xApiKey);
        yield return webRequest.SendWebRequest();
        if (webRequest.result == UnityWebRequest.Result.Success)
        {
            BalanceDataOutput balanceResult = JsonUtility.FromJson<BalanceDataOutput>(webRequest.downloadHandler.text);
            string balance = balanceResult.data.ToString();
            onSuccess(float.Parse(balance));
        }
        else
        {
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
            APIKeyCheckData apiResult = JsonUtility.FromJson<APIKeyCheckData>(webRequest.downloadHandler.text);
            onSuccess(apiResult.status == 200);
        }
        else
        {
            onError(webRequest.error);
            Debug.LogError(webRequest.error);
        }
    }

    public static IEnumerator CoroutineGetFirebaseProfile(MovementProfileScheme aptosProfileData, System.Action<MovementFirebaseAuthDetails> onSuccess, System.Action<string> onError)
    {
        UnityWebRequest webRequest = UnityWebRequest.Put(ROUTES.PROFILE, JsonUtility.ToJson(aptosProfileData));
        webRequest.method = "POST";
        webRequest.SetRequestHeader("Content-Type", "application/json");
        webRequest.SetRequestHeader("x-api-key", LyncManager.Instance.xApiKey);


        yield return webRequest.SendWebRequest();

        if (webRequest.result == UnityWebRequest.Result.Success)
        {
            // Debug.Log(webRequest.downloadHandler.text);
            MovementFirebaseAuthDetails aptosResponse = JsonUtility.FromJson<SupraFirebaseAuthData>(webRequest.downloadHandler.text).data;
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
            onSuccess(webRequest.downloadHandler.text);
        }
        else
        {
            Debug.LogError(webRequest.error);
            onError(webRequest.error);
        }
    }

    public static IEnumerator CoroutineLoginSendAnalytics(string ApiKey, string walletAddress, string loginMethod)
    {
        AnalyticsData jsonObject = new AnalyticsData
        {
            apiKey = ApiKey,
            walletAddress = walletAddress,
            network = LyncManager.Instance.Network == NETWORK.TESTNET ? 2 : 1,
            loginMethod = loginMethod
        };

        var jsonData = JsonUtility.ToJson(jsonObject);
        string RequestURL = "https://server-supra-sdk.lync.world/api/v1/unity/users/login";
        using (UnityWebRequest www = UnityWebRequest.Put(RequestURL, jsonData))
        {
            www.method = "POST";
            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("x-api-key", LyncManager.Instance.xApiKey);
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log("AnalyticsError: " + www.error);
            }
        }
    }

    public static IEnumerator CoroutineSendTransactionsAnalytics(string ApiKey, string walletAddress, string network, string txnHash, string paymentMode)
    {
        TransactionData jsonObject = new TransactionData
        {
            apiKey = ApiKey,
            walletAddress = walletAddress,
            network = LyncManager.Instance.Network == NETWORK.TESTNET ? 2 : 1,
            txnHash = txnHash,
            paymentMode = paymentMode
        };

        var jsonData = JsonUtility.ToJson(jsonObject);

        string RequestURL = "https://server-supra-sdk.lync.world/api/v1/unity/users/transactions";
        using (UnityWebRequest www = UnityWebRequest.Put(RequestURL, jsonData))
        {
            www.method = "POST";
            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("x-api-key", LyncManager.Instance.xApiKey);
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log("AnalyticsError: " + www.error);
            }
        }
    }
}
