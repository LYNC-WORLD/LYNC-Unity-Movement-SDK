using LYNC;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;

public class API
{
    public static string BackendUrl = "http://localhost:5000";

    public delegate void OnSuccess(TransactionData tsxData);
    public delegate void OnError(string error);

    public static IEnumerator CoroutineTransaction(CustomTransaction customTransaction, Action<TransactionData> onSuccess, Action<string> onError)
    {
        string url = BackendUrl + "/api/unity/txn";
        UnityWebRequest webRequest = UnityWebRequest.Put(url, customTransaction.ToJson());
        webRequest.method = "POST";
        webRequest.SetRequestHeader("Content-Type", "application/json");
        webRequest.SetRequestHeader("x-api-key", LyncManager.Instance.xApiKey);
        yield return webRequest.SendWebRequest();

        if (webRequest.result == UnityWebRequest.Result.Success)
        {
            TransactionData tsxData = JsonUtility.FromJson<TransactionData>(webRequest.downloadHandler.text);
            Debug.Log(webRequest.downloadHandler.text);
            onSuccess(tsxData);
        }
        else
        {
            // ErrorDisplay.ShowError(webRequest.error);
            onError(webRequest.downloadHandler.text);
            Debug.Log(webRequest.error);
        }
    }

    public static IEnumerator TempCoroutineTransaction(TRANSACTIONS txnType, AptosWallet aptosWallet, Action<TransactionData> onSuccess, Action<string> onError)
    {
        string url = BackendUrl + "/api/unity/" + txnType.ToString().ToLower();
        UnityWebRequest webRequest = UnityWebRequest.Put(url, JsonUtility.ToJson(aptosWallet));
        webRequest.method = "POST";
        webRequest.SetRequestHeader("Content-Type", "application/json");
        webRequest.SetRequestHeader("x-api-key", LyncManager.Instance.xApiKey);
        yield return webRequest.SendWebRequest();

        if (webRequest.result == UnityWebRequest.Result.Success)
        {
            TransactionData tsxData = JsonUtility.FromJson<TransactionData>(webRequest.downloadHandler.text);
            Debug.Log(webRequest.downloadHandler.text);
            onSuccess(tsxData);
        }
        else
        {
            // ErrorDisplay.ShowError(webRequest.error);
            onError(webRequest.downloadHandler.text);
            Debug.Log(webRequest.error);
        }
    }

    public static IEnumerator CoroutineGetBalance(AptosWallet aptosWallet, Action<float> onSuccess, Action<string> onError)
    {
        string url = BackendUrl + "/api/unity/balance";
        UnityWebRequest webRequest = UnityWebRequest.Put(url, JsonUtility.ToJson(aptosWallet));
        webRequest.method = "POST";
        webRequest.SetRequestHeader("Content-Type", "application/json");
        webRequest.SetRequestHeader("x-api-key", LyncManager.Instance.xApiKey);
        Debug.Log("Sending request " + url);
        yield return webRequest.SendWebRequest();

        if (webRequest.result == UnityWebRequest.Result.Success)
        {
            if (float.TryParse(webRequest.downloadHandler.text, out var balance))
                onSuccess(balance / 100000000);
            else
                onError("Invalid response from the server");
        }
        else
        {
            // ErrorDisplay.ShowError(webRequest.error);
            onError("Error when getting balance");
            Debug.LogError(webRequest.error);
        }
    }

    public static IEnumerator CoroutineSendAnalytics(string ApiKey, string eoaAddress, string smartContractAddress, string chainID)
    {
        AnalyticsData jsonObject = new AnalyticsData
        {
            apiKey = ApiKey,
            eoaAddress = eoaAddress,
            smartContractAddress = smartContractAddress,
            chainId = chainID
        };

        var jsonData = JsonUtility.ToJson(jsonObject);
        string RequestURL = "https://server.lync.world/account-abstraction-unity/insert";
        using (UnityWebRequest www = UnityWebRequest.Put(RequestURL, jsonData))
        {
            www.method = "POST";
            www.SetRequestHeader("Content-Type", "application/json");
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                //Debug.Log(www.error);
            }
            else
            {
                //Debug.Log("www" + www);
                Debug.Log("Error!");
            }
        }

    }

    public static IEnumerator CoroutineCheckAPIKey(string uri, string apiKey, Action<bool> onSuccess, Action<string> onError)
    {
        UnityWebRequest webRequest = UnityWebRequest.Put(uri, JsonUtility.ToJson(new APIKeyCheckBody(apiKey)));
        webRequest.method = "POST";
        webRequest.SetRequestHeader("Content-Type", "application/json");

        Debug.Log("Sending web request...");
        yield return webRequest.SendWebRequest();

        if (webRequest.result == UnityWebRequest.Result.Success)
        {
            //Debug.Log(webRequest.downloadHandler.text);
            APIKeyCheckData apiResult = JsonUtility.FromJson<APIKeyCheckData>(webRequest.downloadHandler.text);
            onSuccess(apiResult.status == 200);
        }
        else
        {
            Debug.Log("err");
            //ErrorDisplay.ShowError(webRequest.downloadHandler.text);
            onError(webRequest.error);
            Debug.LogError(webRequest.error);
        }
    }

    public static IEnumerator CoroutineGetAptosProfile(string uri, AptosProfileData aptosProfileData, Action<AptosWallet> onSuccess, Action<string> onError)
    {
        UnityWebRequest webRequest = UnityWebRequest.Put(uri, JsonUtility.ToJson(aptosProfileData));
        webRequest.method = "POST";
        webRequest.SetRequestHeader("Content-Type", "application/json");
        webRequest.SetRequestHeader("x-api-key", LyncManager.Instance.xApiKey);


        Debug.Log($"Sending web request GET profile email: [{aptosProfileData.email}] - firebaseUid: [{aptosProfileData.firebaseUid}]");
        yield return webRequest.SendWebRequest();

        if (webRequest.result == UnityWebRequest.Result.Success)
        {
            //Debug.Log(webRequest.downloadHandler.text);
            AptosServerResponse aptosResponse = JsonUtility.FromJson<AptosServerResponse>(webRequest.downloadHandler.text);
            onSuccess(aptosResponse.data);
        }
        else
        {
            onError(webRequest.error);
        }
    }
}

public class ApiKeyValidator
{
    public bool IsValid
    {
        get { return this.status == 200; }
    }
    public int status;
}

[System.Serializable]
public class TransactionData
{
    public string message;
    public bool success;
    public int status;
    public TransactionDataDetails data;

    [System.Serializable]
    public class TransactionDataDetails
    {
        public string transactionHash;
    }
}

[System.Serializable]
public class APIKeyCheckData
{
    public int status;
}

public class APIKeyCheckBody
{
    public string apiKey;

    public APIKeyCheckBody(string apiKey)
    {
        this.apiKey = apiKey;
    }

    public APIKeyCheckBody() { }
}

[Serializable]
public class AnalyticsData
{
    public string apiKey;
    public string eoaAddress;
    public string smartContractAddress;

    public string chainId;
}

public enum TRANSACTIONS
{
    FUND, MINT, REFUND
}