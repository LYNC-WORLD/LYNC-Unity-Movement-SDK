using LYNC.Wallet;
using LYNC.Transactions;
using UnityEngine;

namespace LYNC
{
    [RequireComponent(typeof(DeepLinkRegistration))]
    public class LyncManager : MonoBehaviour
    {
        public static LyncManager Instance { private set; get; }
        public WalletAuth WalletAuth { private set; get; }
        public TransactionsManager TransactionsManager { private set; get; }
        public DeepLinkManager DeepLinkManager { private set; get; }
        public static event System.Action<LyncManager> onLyncReady;

        //
        public string LyncAPIKey;
        public string xApiKey { private set; get; } = "42a1d1edcca5f7ef899566fcaf19e14b8cbb64dc5e625d2f52fc890ab8455bb103b48160811b3b3fdb334de7446a9667ba4f24df16b8816233d4d76160d4dd96";

        [Space]
        public NETWORK Network = NETWORK.TESTNET;
        public bool SponsorTransaction = false;

        //
        private static readonly string apiKeyValidationUrl = "https://userservices.lync.world/api/v1/projects/verifyKey?apiKey=";

        // 
        public static readonly string BaseFrontEndURL = "https://login-aptos-sdk.lync.world/";
        public static readonly string BaseServerURL = "https://server-aptos-sdk.lync.world";

        // public static readonly string BaseFrontEndURL = "http://localhost:5173";
        // public static readonly string BaseServerURL = "http://localhost:5001";

        [Space]
        [Header("Login options")]
        public bool LoginOptionFirebase = true;
        public bool LoginOptionPontem = true;
        public bool LoginOptionKeyless = true;
        public string clientId;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                Init();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void Init()
        {
            string savedAuthType = PlayerPrefs.GetString("_savedAuthType", "");
            System.Enum.TryParse(savedAuthType, true, out AUTH_TYPE authType);
            AuthBase.AuthType = authType;

            void OnAPIKeyValidation(bool isValidAPIKey)
            {
                try
                {
                    if (!isValidAPIKey)
                    {
                        Debug.LogError("Invalid API Key. You are not allowed to use LYNC SDK.");
                        return;
                    }
                    Debug.Log("Valid API key");

                    if (WalletAuth.Instance == null)
                        WalletAuth = new WalletAuth();
                    if (DeepLinkManager.Instance == null)
                        DeepLinkManager = new DeepLinkManager(name);

                    // WalletAuth
                    WalletAuth = WalletAuth.Instance;

                    // TransactionsManager
                    TransactionsManager = new TransactionsManager();

                    // Fire ready event if there are listeners
                    onLyncReady?.Invoke(this);
                }
                catch (System.Exception e)
                {
                    Debug.LogException(e);
                }
            }

            void OnAPIKeyWebRequestError(string error)
            {
                Debug.LogError(error);
            }

            // Debug.Log("Checking API key...");
            StartCoroutine(API.CoroutineCheckAPIKey(apiKeyValidationUrl+LyncAPIKey, OnAPIKeyValidation, OnAPIKeyWebRequestError));
        }

        public void HandleWebGLMessage(string message)
        {
            DeepLinkManager.HandleMessage(message);
        }

        // C76FCFCF99C1A09FAA1ED2F727943E18

        public void SendLoginAnalytics(string WalletAddress, string loginMethod)
        {
            StartCoroutine(API.CoroutineLoginSendAnalytics(LyncAPIKey, WalletAddress, (Network).ToString(), loginMethod));
        }

        public void SendTransactionAnalytics(string WalletAddress, string TransactionHash, string PaymentMode)
        {
            StartCoroutine(API.CoroutineSendTransactionsAnalytics(LyncAPIKey, WalletAddress, (Network).ToString(), TransactionHash, PaymentMode));
        }
    }
}

