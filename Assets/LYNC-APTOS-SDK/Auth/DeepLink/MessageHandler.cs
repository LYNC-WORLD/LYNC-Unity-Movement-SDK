using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace LYNC.DeepLink
{
    public class MessageHandler
    {
        public string MessagePath;
        public string MessageData;

        private static Dictionary<string, System.Delegate> registeredEvents = new Dictionary<string, System.Delegate>();

        public void HandleMessage(string url)
        {
            string unescapedUrl = System.Uri.UnescapeDataString(url);
            string messagePath = unescapedUrl.Substring(unescapedUrl.IndexOf("://") + 3);
            messagePath = messagePath.Substring(0, messagePath.IndexOf("?"));
            messagePath = messagePath.Replace("/", "");
            string rawJson = unescapedUrl.Substring(unescapedUrl.IndexOf("?") + 1);

            MessagePath = messagePath;
            MessageData = Utils.FromBase64(rawJson);

            HandleEvents();
        }

        public void HandleAuthMessage(System.Action<AuthBase> authCallback)
        {
            AuthBase authBase = ExtractAndSaveWalletFromDLMessage();
            authCallback(authBase);
        }

        private void HandleTransactionMessage(System.Action<TransactionResult> transactionCallback)
        {
            transactionCallback(JsonUtility.FromJson<TransactionResult>(MessageData));
        }

        public class TempAuthData { public string authType; }
        public class PontemData { public string publicAddress; }

        public AuthBase ExtractAndSaveWalletFromDLMessage()
        {
            TempAuthData tempAuthData = JsonUtility.FromJson<TempAuthData>(MessageData);
            // Save wallet
            AuthBase authBase;
            switch (tempAuthData.authType)
            {
                case "firebase":
                    AuthBase.AuthType = AUTH_TYPE.FIREBASE;
                    AptosAuthData aptosAuthData = JsonUtility.FromJson<AptosAuthData>(MessageData);
                    authBase = new FirebaseAuth(aptosAuthData);
                    break;
                case "pontem":
                    AuthBase.AuthType = AUTH_TYPE.PONTEM;
                    PontemData pontemData = JsonUtility.FromJson<PontemData>(MessageData);
                    authBase = new PontemAuth(pontemData.publicAddress);
                    break;
                default:
                    throw new System.Exception("Unknown auth type");
            }
            return authBase;
        }

        public static void AddAuthListener(System.Action<AuthBase> callback)
        {
            registeredEvents.Remove("auth");
            registeredEvents.Add("auth", callback);
            Debug.Log("Listener added for: " + "auth");
        }

        public static void AddTransactionListener(Transaction transaction, System.Action<TransactionResult> callback)
        {
            registeredEvents.Add(transaction.transactionId, callback);
            Debug.Log("Listener added for transactionId: " + transaction.transactionId);
        }

        private void HandleEvents()
        {
            Debug.Log("Handling this message: " + MessageData);
            if (registeredEvents.TryGetValue("auth", out var authCallback))
            {
                HandleAuthMessage(authCallback as System.Action<AuthBase>);
                registeredEvents.Remove("auth");
            }

            if (MessageData.IndexOf("transactionId") != -1)
            {
                string transactionId = JsonUtility.FromJson<TransactionResult>(MessageData).transactionId;
                if (registeredEvents.TryGetValue(transactionId, out var transactionCallback))
                {
                    HandleTransactionMessage(transactionCallback as System.Action<TransactionResult>);
                    registeredEvents.Remove(transactionId);
                }
            }
        }
    }
}