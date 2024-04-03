using System.Collections.Generic;
using LYNC;
using LYNC.Wallet;
using UnityEngine;

public class PontemMobileDeepLinkHandler
{
    public PontemMobileAuthInScheme authData;
    public string transactionResult;

    public PontemMobileDeepLinkHandler() { }

    public void HandleAuthData(string encodedRawData)
    {
        string encodedBase64 = encodedRawData.Substring(encodedRawData.IndexOf("?account=") + "?account=".Length);
        string decoded = Utils.FromBase64(encodedBase64);
        decoded = decoded.Replace("{\"account\":", "").Replace("}}", "}");
        authData = JsonUtility.FromJson<PontemMobileAuthInScheme>(decoded);
    }

    public void HandleTransactionData(string encodedRawData)
    {
        transactionResult = encodedRawData.Substring(encodedRawData.IndexOf("?response=") + "?response=".Length);
    }
}

public class PontemMobileAuthInScheme
{
    public string address;
    public string publicKey;
}

public class PontemMobileAuthOutScheme
{
    public string name;
    public string logoUrl;
    public string redirectLink;

    public PontemMobileAuthOutScheme()
    {
        name = Application.productName;
        logoUrl = "";
        redirectLink = DeepLinkRegistration.DeepLinkUrl + "://";
    }

    public string ToBase64()
    {
        return Utils.ToBase64(JsonUtility.ToJson(this));
    }
}

public class PontemMobileTransactionOutScheme
{
    public string type;
    public string function;
    public List<string> arguments = new List<string>();
    public List<string> type_arguments = new List<string>();

    public PontemMobileTransactionOutScheme(Transaction transaction)
    {
        type = "entry_function_payload";
        function = $"{transaction.contractAddress}::{transaction.contractName}::{transaction.functionName}";
        arguments = MapArgs(transaction.arguments);
    }

    private List<string> MapArgs(List<TransactionArgument> args)
    {
        List<string> _arguments = new List<string>();
        for (int i = 0; i < args.Count; i++)
        {
            _arguments.Add(args[i].argument);
        }

        return _arguments;
    }

    public string ToBase64()
    {
        return Utils.ToBase64(JsonUtility.ToJson(this));
    }
}