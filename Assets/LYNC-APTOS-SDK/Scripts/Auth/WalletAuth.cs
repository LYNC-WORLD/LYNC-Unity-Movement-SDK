using LYNC.DeepLink;
using UnityEngine;

namespace LYNC.Wallet
{
    public class WalletAuth
    {
        public static WalletAuth Instance { private set; get; }
        public static event System.Action<string> WalletConnectionRequested;

        public WalletAuth()
        {
            Instance = this;
        }

        public void ConnectWallet(System.Action<AuthBase> onSuccess = null)
        {
            async void _onSuccess(AuthBase authBase)
            {
                try
                {
                    if (AuthBase.Instance is FirebaseAuth)
                        await (authBase as FirebaseAuth).AptosFirebaseAuthData.UpdateBalance();
                }
                catch (System.Exception e)
                {
                    Debug.LogException(e);
                }
                finally
                {
                    onSuccess(authBase);
                }
            }

            MessageHandler.AddListener<AuthBase>(_onSuccess);

            string url = LyncManager.BaseFrontEndURL + "/auth?scheme=" + DeepLinkRegistration.DeepLinkUrl + "&network=" + (int)LyncManager.Instance.Network;
            // For mobile platforms, used to add app_info for interacting with Pontem Mobile App
            // app_info is also used in the front end to redirect to Pontem Mobile App
            if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
            {
                PontemMobileAuthOutScheme pontemMobile = new PontemMobileAuthOutScheme();
                url += "&app_info=" + pontemMobile.ToBase64();
            }

            // Append login options
            url += Utils.GetLoginOptionsUrlFormat();
            if (!LyncManager.Instance.LoginOptionFirebase && !LyncManager.Instance.LoginOptionKeyless && !LyncManager.Instance.LoginOptionPontem)
                Debug.LogWarning("Warning, no login option was selected. Please select at least one login option from LyncManager Prefab.");

            DeepLinkManager.Instance.StartBrowserProcess(url);
        }

        public void Logout()
        {
            AuthBase.Logout();
        }
    }
}
