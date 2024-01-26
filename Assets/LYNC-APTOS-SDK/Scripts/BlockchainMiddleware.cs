using System.Collections.Generic;
using System.Threading.Tasks;
using LYNC.Wallet;
using UnityEngine;

namespace LYNC.Transactions
{
    public class BlockchainMiddleware : MonoBehaviour
    {
        [HideInInspector] public string rpcUrl, dappAPIKey;
        protected WalletData walletData;

        public async Task<bool> IsReady()
        {
            try
            {
                await walletData.Load();
                return walletData.WalletConnected;
            }
            catch (System.Exception err)
            {
                Debug.LogError(err);
                throw err;
            }

        }

        private async void Start()
        {
            walletData = await WalletData.TryLoadSavedWallet();
        }

        public async void SendTransaction(TRANSACTIONS transactionType, System.Action<TransactionData> onSuccess = null, System.Action<string> onError = null)
        {
            try
            {
                if (!await IsReady()) throw new System.Exception("Wallet not connected!");

                StartCoroutine(API.CoroutineTransaction(transactionType, walletData.AptosWallet, onSuccess, onError));
            }
            catch (System.Exception e)
            {
                Debug.LogError(e);
            }
        }
    }
}