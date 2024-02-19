using LYNC.DeepLink;
using UnityEngine;

namespace LYNC.Wallet
{
    public class WalletAuth : MonoBehaviour
    {
        public static WalletAuth Instance { private set; get; }
        public static event System.Action<string> WalletConnectionRequested;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void ConnectWallet(string loginUrl, System.Action<AuthBase> onSuccess = null)
        {
            async void _onSuccess(AuthBase authBase)
            {
                if (AuthBase.Instance is FirebaseAuth)
                    await (authBase as FirebaseAuth).AptosAuthData.UpdateBalance();

                onSuccess(authBase);
            }

            MessageHandler.AddAuthListener(_onSuccess);
            DeepLinkManager.Instance.StartBrowserProcess(loginUrl + "?scheme=" + DeepLinkRegistration.DeepLinkUrl);
        }

        public void Logout()
        {
            AuthBase.Logout();
        }
    }
}
