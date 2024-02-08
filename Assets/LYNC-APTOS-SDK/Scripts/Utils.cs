using System;
using System.Collections.Generic;
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
            publicAddress = WalletData.ConnectedWalletInstance.AptosWallet.publicKey;
            privateAddress = WalletData.ConnectedWalletInstance.AptosWallet.privateKey;
            firebaseUid = WalletData.ConnectedWalletInstance.AptosFirebaseUid;
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