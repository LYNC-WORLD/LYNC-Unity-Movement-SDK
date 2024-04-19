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
        public string xApiKey;

        [Space]
        public NETWORK Network = NETWORK.TESTNET;
        public bool SponsorTransaction = false;

        //
        private static readonly string apiKeyValidationUrl = "https://server.lync.world/user/check_api_key";

        // 
        public static readonly string BaseFrontEndURL = "https://aptos-collection-git-kashyap-deeplink-fix-lync-world.vercel.app";
        public static readonly string BaseServerURL = "https://server-aptos-sdk.lync.world";

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
                        DeepLinkManager = new DeepLinkManager();

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

            Debug.Log("Checking API key...");
            StartCoroutine(API.CoroutineCheckAPIKey(apiKeyValidationUrl, LyncAPIKey, OnAPIKeyValidation, OnAPIKeyWebRequestError));
        }

        // C76FCFCF99C1A09FAA1ED2F727943E18
    }
}

