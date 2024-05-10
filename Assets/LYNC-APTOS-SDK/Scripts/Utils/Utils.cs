using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LYNC.Wallet;
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

    public class AptosFirebaseSavedProfile
    {
        public string message;
        public bool success;
        public int status;
        public AptosFirebaseAuthData data;
    }

    [System.Serializable]
    public class AptosFirebaseAuthData
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

    public class AptosProfileScheme
    {
        public string email;
        public string firebaseUid;
        public string network;

        public AptosProfileScheme(string email, string firebaseUid)
        {
            this.email = email;
            this.firebaseUid = firebaseUid;
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
        [HideInInspector] public string publicAddress;
        [HideInInspector] public string privateAddress;
        [HideInInspector] public string firebaseUid;
        [HideInInspector] public bool usePaymaster;
        [HideInInspector] public string network;
        [HideInInspector] public string dataId;

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
            network = LyncManager.Instance.Network.ToString();

            if (authBase is FirebaseAuth)
            {
                publicAddress = authBase.PublicAddress;
                privateAddress = (authBase as FirebaseAuth).AptosFirebaseAuthData.privateKey;
                firebaseUid = (authBase as FirebaseAuth).FirebaseUid;
            }
            if (authBase is KeylessAuth)
            {
                privateAddress = (authBase as KeylessAuth).KeyPairPrivateKey;
                dataId = (authBase as KeylessAuth).dataId;
                publicAddress = authBase.PublicAddress;
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

    public enum ARGUMENT_TYPE { STRING = 0, NUMBER, BYTEARRAY }
    public enum NETWORK { MAINNET = 1, TESTNET = 2, DEVNET = 3 }

    [Serializable]
    public class TransactionArgument
    {
        public string argument;
        public ARGUMENT_TYPE type;
    }

    public class TransactionResult
    {
        public bool success;
        public string response;
        public string hash;
        public string transactionId;
        public string error;
        public string value;
    }
}