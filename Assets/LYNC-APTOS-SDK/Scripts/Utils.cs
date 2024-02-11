using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LYNC.Wallet;
using UnityEngine;

namespace LYNC
{
    public class Utils
    {
        public static string MapArgsToString(List<string> args)
        {
            if (args == null || args.Count == 0) return "";

            string temp = "";
            foreach (var arg in args)
            {
                temp += arg + ",";
            }

            temp = temp.Substring(0, temp.Length - 1);
            return temp;
        }
    }

    [System.Serializable]
    public class AptosServerResponse
    {
        public string message;
        public int status;
        public bool success;
        public AptosWallet data;
    }

    [System.Serializable]
    public class AptosWallet
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
            LyncManager.Instance.StartCoroutine(API.CoroutineGetBalance(this, res =>
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

    public class AptosProfileData
    {
        public string email;
        public string firebaseUid;

        public AptosProfileData(string email, string firebaseUid)
        {
            this.email = email;
            this.firebaseUid = firebaseUid;
        }
    }

    [Serializable]
    public class CustomTransaction
    {
        public string contractAddress;
        public string contractName;
        public string functionName;
        public List<TransactionArgument> arguments;
        [HideInInspector] public string args;

        [HideInInspector] public string publicAddress;
        [HideInInspector] public string privateAddress;
        [HideInInspector] public string firebaseUid;

        public CustomTransaction(string contractAddress, string contractName, string functionName, List<Dictionary<string, string>> args)
        {
            // this.arguments = args;
            this.functionName = functionName;
            this.contractName = contractName;
            this.contractAddress = contractAddress;
        }

        public CustomTransaction(string contractAddress, string contractName, string functionName)
        {
            this.functionName = functionName;
            this.contractName = contractName;
            this.contractAddress = contractAddress;
        }

        private void PopulateGenericData()
        {
            publicAddress = FirebaseAuth.Instance.AptosWallet.publicKey;
            privateAddress = FirebaseAuth.Instance.AptosWallet.privateKey;
            firebaseUid = FirebaseAuth.Instance.FirebaseUid;
        }

        public string ToJson()
        {
            PopulateGenericData();
            return JsonUtility.ToJson(this);
        }
    }

    public enum ARGUMENT_TYPE { STRING, NUMBER, BYTEARRAY }

    [Serializable]
    public class TransactionArgument
    {
        public string argument;
        public ARGUMENT_TYPE type;
    }
}