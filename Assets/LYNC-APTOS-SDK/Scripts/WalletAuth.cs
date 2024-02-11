using UnityEngine;

namespace LYNC.Wallet
{
    public class WalletAuth : MonoBehaviour
    {
        public static WalletAuth Instance { private set; get; }
        public static event System.Action<string, System.Action<AuthBase>> walletConnectionRequested;

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
                switch (AuthBase.AuthType)
                {
                    case AUTH_TYPE.FIREBASE:
                        try
                        {
                            await ((FirebaseAuth)authBase).AptosWallet.UpdateBalance();
                            onSuccess(authBase);
                        }
                        catch (System.Exception e)
                        {
                            Debug.LogError(e);
                        }
                        break;
                    default:
                        break;
                }
            }
            walletConnectionRequested?.Invoke(loginUrl, _onSuccess);
        }

        public void Logout()
        {
            AuthBase.Logout();
        }
    }
}
