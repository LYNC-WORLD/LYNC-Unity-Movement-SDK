using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace LYNC.DeepLink
{
    public class MessageHandler
    {
        public string MessagePath;
        public string MessageData;

        private PontemMobileDeepLinkHandler pontemMobile = new PontemMobileDeepLinkHandler();

        private static Dictionary<string, System.Delegate> registeredEvents = new Dictionary<string, System.Delegate>();

        public void HandleMessage(string url)
        {
            string unescapedUrl = System.Uri.UnescapeDataString(url);
            string messagePath = unescapedUrl.Substring(unescapedUrl.IndexOf("://") + 3);
            messagePath = messagePath.Substring(messagePath.IndexOf("?") + 1, messagePath.IndexOf("=") - 1);
            MessagePath = messagePath.Replace("=", "");

            switch (MessagePath)
            {
                case DEEPLINK_MESSAGE_PATH.PONTEM_MOBILE_AUTH:
                case DEEPLINK_MESSAGE_PATH.PONTEM_MOBILE_TRANSACTION:
                    MessageData = unescapedUrl;
                    break;
                case DEEPLINK_MESSAGE_PATH.KEYLESS_AUTH:
                case DEEPLINK_MESSAGE_PATH.PONTEM_BROWSER_TRANSACTION:
                case DEEPLINK_MESSAGE_PATH.PONTEM_BROWSER_AUTH:
                case DEEPLINK_MESSAGE_PATH.FIREBASE:
                    string rawJson = unescapedUrl.Substring(unescapedUrl.IndexOf("=") + 1);
                    MessageData = Utils.FromBase64(rawJson);
                    break;
            }

            HandleEvents();
        }

        public void HandleAuthMessage(System.Action<AuthBase> authCallback)
        {
            AuthBase authBase = ExtractAndSaveWalletFromDLMessage();
            authCallback(authBase);
        }

        public class TempAuthData { public string authType; }
        public class PontemData { public string publicAddress; }
        public class KeylessData { public string accountAddress; public int expirationDateSeconds; public string publicKey; public string privateKey; public string dataId; }

        public AuthBase ExtractAndSaveWalletFromDLMessage()
        {
            // Save wallet
            AuthBase authBase;
            switch (MessagePath)
            {
                case DEEPLINK_MESSAGE_PATH.FIREBASE:
                    AuthBase.AuthType = AUTH_TYPE.FIREBASE;
                    AptosFirebaseAuthData AptosFirebaseAuthData = JsonUtility.FromJson<AptosFirebaseAuthData>(MessageData);
                    LyncManager.Instance.SendLoginAnalytics(AptosFirebaseAuthData.publicKey, "Firebase");
                    authBase = new FirebaseAuth(AptosFirebaseAuthData);
                    break;
                case DEEPLINK_MESSAGE_PATH.PONTEM_MOBILE_AUTH:
                    AuthBase.AuthType = AUTH_TYPE.PONTEM;
                    pontemMobile.HandleAuthData(MessageData);
                    LyncManager.Instance.SendLoginAnalytics(pontemMobile.authData.address, "Pontem");
                    authBase = new PontemAuth(pontemMobile.authData.address);
                    break;
                case DEEPLINK_MESSAGE_PATH.PONTEM_BROWSER_AUTH:
                    AuthBase.AuthType = AUTH_TYPE.PONTEM;
                    PontemData pontemData = JsonUtility.FromJson<PontemData>(MessageData);
                    LyncManager.Instance.SendLoginAnalytics(pontemData.publicAddress, "Pontem");
                    authBase = new PontemAuth(pontemData.publicAddress);
                    break;
                case DEEPLINK_MESSAGE_PATH.KEYLESS_AUTH:
                    AuthBase.AuthType = AUTH_TYPE.KEYLESS;
                    KeylessData keylessData = JsonUtility.FromJson<KeylessData>(MessageData);
                    authBase = new KeylessAuth(keylessData.accountAddress, keylessData.publicKey, keylessData.privateKey, keylessData.expirationDateSeconds, keylessData.dataId);
                    Debug.Log(MessageData);
                    Debug.Log(keylessData.dataId);
                    break;

                default:
                    throw new System.Exception("Unknown auth type");
            }
            return authBase;
        }

        public static void AddListener<GenericType>(System.Action<GenericType> callback, object genericParam = null)
        {
            if (typeof(GenericType) == typeof(AuthBase))
            {
                registeredEvents.Remove("auth");
                registeredEvents.Add("auth", callback);
            }
            else if (typeof(GenericType) == typeof(TransactionResult))
            {
                registeredEvents.Remove((genericParam as Transaction).transactionId);
                registeredEvents.Add((genericParam as Transaction).transactionId, callback);
                // GameObject.FindGameObjectWithTag("debug").GetComponent<TMPro.TMP_Text>().text += "\nListener added for transactionId: " + (genericParam as Transaction).transactionId;
            }
            else
            {
                throw new System.Exception("Unknown GenericType");
            }
        }

        private void HandleEvents()
        {
            if (registeredEvents.TryGetValue("auth", out var authCallback))
            {
                HandleAuthMessage(authCallback as System.Action<AuthBase>);
                registeredEvents.Remove("auth");
            }

            // Pontem browser transaction
            else if (MessagePath == DEEPLINK_MESSAGE_PATH.PONTEM_BROWSER_TRANSACTION)
            {
                string transactionId = JsonUtility.FromJson<TransactionResult>(MessageData).transactionId;
                string transactionHash = JsonUtility.FromJson<TransactionResult>(MessageData).hash;
                if (registeredEvents.TryGetValue(transactionId, out var transactionCallback))
                {
                    (transactionCallback as System.Action<TransactionResult>)(JsonUtility.FromJson<TransactionResult>(MessageData));
                    LyncManager.Instance.SendTransactionAnalytics(AuthBase.Instance.PublicAddress, transactionHash, "UserPaid");
                    registeredEvents.Remove(transactionId);
                }
                else
                {
                    throw new System.Exception("Attempting to manage a Pontem browser transaction, but no registered callbacks were found!");
                }
            }

            // Pontem mobile transaction
            else if (MessagePath == DEEPLINK_MESSAGE_PATH.PONTEM_MOBILE_TRANSACTION)
            {
                if (registeredEvents.TryGetValue(DEEPLINK_MESSAGE_PATH.PONTEM_MOBILE_TRANSACTION, out var transactionCallback))
                {
                    pontemMobile.HandleTransactionData(MessageData);
                    TransactionResult transactionResult = new TransactionResult();
                    transactionResult.success = pontemMobile.transactionResult == "approved";
                    transactionResult.response = pontemMobile.transactionResult;
                    (transactionCallback as System.Action<TransactionResult>)(transactionResult);
                    LyncManager.Instance.SendTransactionAnalytics(AuthBase.Instance.PublicAddress, transactionResult.hash, "UserPaid");
                    registeredEvents.Remove(DEEPLINK_MESSAGE_PATH.PONTEM_MOBILE_TRANSACTION);
                }
                else
                {
                    throw new System.Exception("Attempting to manage a Pontem mobile transaction, but no registered callbacks were found!");
                }
            }
            else
            {
                throw new System.Exception("Attempting to handle non existing event - MessagePath = " + MessagePath + " - MessageData = " + MessageData);
            }
        }
    }

    public class DEEPLINK_MESSAGE_PATH
    {
        public const string PONTEM_MOBILE_AUTH = "account";
        public const string PONTEM_MOBILE_TRANSACTION = "response";
        public const string PONTEM_BROWSER_AUTH = "pontem-browser";
        public const string PONTEM_BROWSER_TRANSACTION = "pontem-browser-transaction";
        public const string FIREBASE = "firebase";
        public const string KEYLESS_AUTH = "keyless-auth";
    }
}