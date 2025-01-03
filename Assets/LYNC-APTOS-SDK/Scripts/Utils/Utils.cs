using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace LYNC
{
    public class Utils
    {
        public static string ToBase64(string data)
        {
            byte[] bytesToEncode = System.Text.Encoding.UTF8.GetBytes(data);
            string encodedString = Convert.ToBase64String(bytesToEncode);

            return encodedString;
        }

        public static string FromBase64(string encoded)
        {
            byte[] bytesToDecode = Convert.FromBase64String(encoded);
            string decodedString = System.Text.Encoding.UTF8.GetString(bytesToDecode);

            return decodedString;
        }

        public static string GetLoginOptionsUrlFormat() =>
             "&loFirebase=" + LyncManager.Instance.LoginOptionFirebase + "&loPontem=" + LyncManager.Instance.LoginOptionPontem + "&loKeyless=" + LyncManager.Instance.LoginOptionKeyless;
    }
    [Serializable]
    public class SupraFirebaseAuthData{
        public int status;
        public bool success;
        public String message;
        public SupraFirebaseAuthDetails data;
    }
    [System.Serializable]
    public class SupraFirebaseAuthDetails
    {
        public bool isFunded;
        public string mintingHash;
        public string fundingHash;
        public string _id;
        public string firebaseUid;
        public string email;
        public string name;
        public string avatar;
        public string lastLoginAt;
        public string providerId;
        public string createdAt;
        public string updatedAt;
        public string privateKey;
        public string publicKey;
        public float balance;

        public async Task<float> UpdateBalance()
        {
            var tcs = new TaskCompletionSource<float>();
            LyncManager.Instance.StartCoroutine(API.CoroutineGetBalance(this.publicKey, res =>
            {
                balance = res;
                tcs.SetResult(res);
            }, err =>
            {
                tcs.SetException(new System.Exception(err));
            }));

            return await tcs.Task;
        }
    }

    public class SupraProfileScheme
    {
        public string email;
        // public string firebaseUid;
        public string network;

        public SupraProfileScheme(string email, string firebaseUid)
        {
            this.email = email;
            // this.firebaseUid = firebaseUid;
            this.network = ((int)LyncManager.Instance.Network).ToString(); ;
        }
    }

    [Serializable]
    public class Transaction
    {
        public string contractAddress;
        public string contractName;
        public string functionName;
        public List<TransactionArgument> arguments;

        [HideInInspector] public string transactionId;
        [HideInInspector] public string accountAddress;
        [HideInInspector] public string privateAddress;
        // [HideInInspector] public string firebaseUid;
        [HideInInspector] public bool usePaymaster;
        [HideInInspector] public int network;
        // [HideInInspector] public string dataId;
        [HideInInspector] public string apiKey;

        private AuthBase authBase;

        public Transaction(string contractAddress, string contractName, string functionName, List<TransactionArgument> args)
        {
            arguments = args;
            this.functionName = functionName;
            this.contractName = contractName;
            this.contractAddress = contractAddress;
        }

        public Transaction(string contractAddress, string contractName, string functionName)
        {
            this.functionName = functionName;
            this.contractName = contractName;
            this.contractAddress = contractAddress;
        }

        private void AppendAuthData()
        {
            authBase = AuthBase.Instance;
            usePaymaster = LyncManager.Instance.SponsorTransaction;
            network = LyncManager.Instance.Network == NETWORK.TESTNET ? 2 : 1;
            apiKey = LyncManager.Instance.LyncAPIKey;
            if (authBase is FirebaseAuth)
            {
                accountAddress = authBase.PublicAddress;
                privateAddress = (authBase as FirebaseAuth).supraFirebaseAuthDetails.privateKey;
                // firebaseUid = (authBase as FirebaseAuth).FirebaseUid;
            }
            if (authBase is KeylessAuth)
            {
                privateAddress = (authBase as KeylessAuth).KeyPairPrivateKey;
                // dataId = (authBase as KeylessAuth).dataId;
                // publicAddress = authBase.PublicAddress;
            }
            if (authBase is PontemAuth)
            {

            }
        }

        public string ToJson()
        {
            AppendAuthData();
            return JsonUtility.ToJson(this);
        }
    }

    public enum ARGUMENT_TYPE { STRING = 1, NUMBER, BYTEARRAY }
    public enum NETWORK { MAINNET = 1, TESTNET = 2, DEVNET = 3 }

    [Serializable]
    public class TransactionArgument
    {
        public string argument;
        public ARGUMENT_TYPE type;
        public int bitSize = 8;
    }

    public class TransactionResult
    {
        public int status;
        public bool success;
        public string response;
        public string hash;
        // public TransactionData data;
        public string transactionId;
        public string error;
        public string value;
    }

    public class ApiKeyValidator
    {
        public bool IsValid
        {
            get { return this.status == 200; }
        }
        public int status;
    }

    [Serializable]
    public class ServerBasedTransactionFeedback
    {
        public string message;
        public bool success;
        public int status;
        public ServerBasedTransactionFeedbackDetails data;

        [Serializable]
        public class ServerBasedTransactionFeedbackDetails
        {
            public TransactionHashClass transactionHash;
        }

        public TransactionResult ToTransactionResult()
        {
            var temp = new TransactionResult
            {
                success = success,
                hash = data.transactionHash.txHash
            };
            return temp;
        }
    }
    [Serializable]
    public class TransactionHashClass{
        public string status;
        public string txHash;
    }

    [Serializable]
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
    public class BalanceData
    {
        public string network;
        public string publicKey;
    }

    [Serializable]
    public class BalanceDataOutput
    {
        public int status;
        public bool success;
        public bool message;
        public BalanceDataOutputData data;
    }
    [Serializable]
    public class BalanceDataOutputData{
        public String data;
    }

    [Serializable]
    public class AnalyticsData
    {
        public string apiKey;
        public string walletAddress;
        public string network;
        public string loginMethod;
    }

    [Serializable]
    public class TransactionData
    {
        public string apiKey;
        public string walletAddress;
        public string network;
        public string txnHash;
        public string paymentMode;
    }
    [Serializable]
    public class ViewTransection{
        public string contractAddress;
        public string contractName;
        public string functionName;
        public string network;
        public bool usePaymaster;
        public List<TransactionArgument> arguments;
    }
}