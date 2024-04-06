using LYNC;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class ROUTES
{
    public static string GENERIC_TRANSACTION = LyncManager.BaseServerURL + "/api/unity/" + "txn2";
    public static string FUND = LyncManager.BaseServerURL + "/api/unity/" + "fund";
    public static string MINT = LyncManager.BaseServerURL + "/api/unity/" + "mint";
    public static string REFUND = LyncManager.BaseServerURL + "/api/unity/" + "refund";
    public static string BALANCE = LyncManager.BaseServerURL + "/api/unity/" + "balance";
    public static string PROFILE = LyncManager.BaseServerURL + "/api/users/" + "profile";
    public static string MOBILE_TRANSACTION = LyncManager.BaseServerURL + "/api/unity/" + "mobile-transaction-builder";
}

public class API
{
    public static ROUTES ROUTES;

    public delegate void OnSuccess(ServerBasedTransactionFeedback tsxData);
    public delegate void OnError(string error);

    public static IEnumerator CoroutineTransaction(Transaction customTransaction, System.Action<ServerBasedTransactionFeedback> onSuccess, System.Action<TransactionResult> onError)
    {
        UnityWebRequest webRequest = UnityWebRequest.Put(ROUTES.GENERIC_TRANSACTION, customTransaction.ToJson());
        webRequest.method = "POST";
        webRequest.SetRequestHeader("Content-Type", "application/json");
        webRequest.SetRequestHeader("x-api-key", LyncManager.Instance.xApiKey);
        yield return webRequest.SendWebRequest();

        if (webRequest.result == UnityWebRequest.Result.Success)
        {
            ServerBasedTransactionFeedback tsxData = JsonUtility.FromJson<ServerBasedTransactionFeedback>(webRequest.downloadHandler.text);
            Debug.Log(webRequest.downloadHandler.text);
            onSuccess(tsxData);
        }
        else
        {
            TransactionResult tsxData = JsonUtility.FromJson<TransactionResult>(webRequest.downloadHandler.text);
            tsxData.success = false;
            Debug.Log(webRequest.downloadHandler.text);
            onError(tsxData);
            Debug.Log(webRequest.error);
        }
    }

    public static IEnumerator TempCoroutineTransaction(TRANSACTIONS txnType, AptosFirebaseAuthData aptosWallet, System.Action<ServerBasedTransactionFeedback> onSuccess, System.Action<string> onError)
    {
        string url = LyncManager.BaseServerURL + "/api/unity/" + txnType.ToString().ToLower();
        UnityWebRequest webRequest = UnityWebRequest.Put(url, JsonUtility.ToJson(aptosWallet));
        webRequest.method = "POST";
        webRequest.SetRequestHeader("Content-Type", "application/json");
        webRequest.SetRequestHeader("x-api-key", LyncManager.Instance.xApiKey);
        yield return webRequest.SendWebRequest();

        if (webRequest.result == UnityWebRequest.Result.Success)
        {
            ServerBasedTransactionFeedback tsxData = JsonUtility.FromJson<ServerBasedTransactionFeedback>(webRequest.downloadHandler.text);
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

    public static IEnumerator CoroutineGetBalance(AptosFirebaseAuthData aptosWallet, System.Action<float> onSuccess, System.Action<string> onError)
    {
        string url = LyncManager.BaseServerURL + "/api/unity/balance";
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

    public static IEnumerator CoroutineCheckAPIKey(string uri, string apiKey, System.Action<bool> onSuccess, System.Action<string> onError)
    {
        UnityWebRequest webRequest = UnityWebRequest.Put(uri, JsonUtility.ToJson(new APIKeyCheckBody(apiKey)));
        webRequest.method = "POST";
        webRequest.SetRequestHeader("Content-Type", "application/json");

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

    public static IEnumerator CoroutineGetFirebaseProfile(AptosProfileScheme aptosProfileData, System.Action<AptosFirebaseAuthData> onSuccess, System.Action<string> onError)
    {
        UnityWebRequest webRequest = UnityWebRequest.Put(ROUTES.PROFILE, JsonUtility.ToJson(aptosProfileData));
        webRequest.method = "POST";
        webRequest.SetRequestHeader("Content-Type", "application/json");
        webRequest.SetRequestHeader("x-api-key", LyncManager.Instance.xApiKey);


        Debug.Log($"Sending web request GET profile email: [{aptosProfileData.email}] - firebaseUid: [{aptosProfileData.firebaseUid}]");
        yield return webRequest.SendWebRequest();

        if (webRequest.result == UnityWebRequest.Result.Success)
        {
            Debug.Log(webRequest.downloadHandler.text);
            AptosFirebaseAuthData aptosResponse = JsonUtility.FromJson<AptosFirebaseSavedProfile>(webRequest.downloadHandler.text).data;
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
            Debug.Log(webRequest.downloadHandler.text);
            onSuccess(webRequest.downloadHandler.text);
        }
        else
        {
            Debug.LogError(webRequest.error);
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
public class ServerBasedTransactionFeedback
{
    public string message;
    public bool success;
    public int status;
    public ServerBasedTransactionFeedbackDetails data;

    [System.Serializable]
    public class ServerBasedTransactionFeedbackDetails
    {
        public string transactionHash;
    }

    public TransactionResult ToTransactionResult()
    {
        var temp = new TransactionResult
        {
            success = success,
            hash = data.transactionHash
        };
        return temp;
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

[System.Serializable]
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