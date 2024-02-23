using System.Collections.Generic;
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
            messagePath = messagePath.Substring(0, messagePath.IndexOf("?"));
            messagePath = messagePath.Replace("/", "");
            MessagePath = messagePath;

            // when redirected from pontem mobile, the host/path will be set to "" and should be handled manually
            // based on the message/url content
            // host/path support can be added to this SDK but will it require adding 2 more intent-filters for Android in the PostBuildProcessing class
            if (MessagePath == "")
            {
                if (unescapedUrl.IndexOf("?account=") != -1) // auth
                    MessagePath = DEEPLINK_MESSAGE_PATH.PONTEM_MOBILE_AUTH;

                if (unescapedUrl.IndexOf("?response=") != -1) // transaction
                    MessagePath = DEEPLINK_MESSAGE_PATH.PONTEM_MOBILE_TRANSACTION;
            }

            // Pontem mobile wallet Auth
            switch (MessagePath)
            {
                case DEEPLINK_MESSAGE_PATH.PONTEM_MOBILE_AUTH:
                case DEEPLINK_MESSAGE_PATH.PONTEM_MOBILE_TRANSACTION:
                    MessageData = unescapedUrl;
                    break;
                case DEEPLINK_MESSAGE_PATH.PONTEM_BROWSER_TRANSACTION:
                case DEEPLINK_MESSAGE_PATH.PONTEM_BROWSER_AUTH:
                case DEEPLINK_MESSAGE_PATH.FIREBASE:
                    string rawJson = unescapedUrl.Substring(unescapedUrl.IndexOf("?") + 1);
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

        public AuthBase ExtractAndSaveWalletFromDLMessage()
        {
            // Save wallet
            AuthBase authBase;
            switch (MessagePath)
            {
                case DEEPLINK_MESSAGE_PATH.FIREBASE:
                    AuthBase.AuthType = AUTH_TYPE.FIREBASE;
                    AptosAuthData aptosAuthData = JsonUtility.FromJson<AptosAuthData>(MessageData);
                    authBase = new FirebaseAuth(aptosAuthData);
                    break;
                case DEEPLINK_MESSAGE_PATH.PONTEM_MOBILE_AUTH:
                    AuthBase.AuthType = AUTH_TYPE.PONTEM;
                    pontemMobile.HandleAuthData(MessageData);
                    authBase = new PontemAuth(pontemMobile.authData.address);
                    break;
                case DEEPLINK_MESSAGE_PATH.PONTEM_BROWSER_AUTH:
                    AuthBase.AuthType = AUTH_TYPE.PONTEM;
                    PontemData pontemData = JsonUtility.FromJson<PontemData>(MessageData);
                    authBase = new PontemAuth(pontemData.publicAddress);
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
                registeredEvents.Add((genericParam as Transaction).transactionId, callback);
                Debug.Log("Listener added for transactionId: " + (genericParam as Transaction).transactionId);
                GameObject.FindGameObjectWithTag("debug").GetComponent<TMPro.TMP_Text>().text += "\nListener added for transactionId: " + (genericParam as Transaction).transactionId;
            }
            else
            {
                throw new System.Exception("Unknown GenericType");
            }
        }

        private void HandleEvents()
        {
            Debug.Log("Handling this message: " + MessageData);
            if (registeredEvents.TryGetValue("auth", out var authCallback))
            {
                HandleAuthMessage(authCallback as System.Action<AuthBase>);
                registeredEvents.Remove("auth");
            }

            // Pontem browser transaction
            else if (MessagePath == DEEPLINK_MESSAGE_PATH.PONTEM_BROWSER_TRANSACTION)
            {
                string transactionId = JsonUtility.FromJson<TransactionResult>(MessageData).transactionId;
                if (registeredEvents.TryGetValue(transactionId, out var transactionCallback))
                {
                    (transactionCallback as System.Action<TransactionResult>)(JsonUtility.FromJson<TransactionResult>(MessageData));
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
        public const string PONTEM_MOBILE_AUTH = "pontem-mobile-app-auth";
        public const string PONTEM_MOBILE_TRANSACTION = "pontem-mobile-app-transaction";
        public const string PONTEM_BROWSER_AUTH = "pontem-browser";
        public const string PONTEM_BROWSER_TRANSACTION = "pontem-browser-transaction";
        public const string FIREBASE = "firebase";
    }
}