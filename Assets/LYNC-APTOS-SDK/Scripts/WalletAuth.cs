using UnityEngine;

namespace LYNC.Wallet
{
    public class WalletAuth : MonoBehaviour
    {
        public static WalletAuth Instance { private set; get; }
        public static event System.Action<string, System.Action<WalletData>> walletConnectionRequested;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void ConnectWallet(string loginUrl, System.Action<WalletData> onSuccess = null)
        {
            async void _onSuccess(WalletData walletData)
            {
                try
                {
                    await walletData.GetBalance();
                    Debug.Log("Balance = " + walletData.AptosWallet.balance);
                    onSuccess(walletData);
                }
                catch (System.Exception e)
                {
                    Debug.LogError(e);
                }
            }
            walletConnectionRequested?.Invoke(loginUrl, _onSuccess);
        }

        public void Logout()
        {
            WalletData.Logout();
        }
    }
}
