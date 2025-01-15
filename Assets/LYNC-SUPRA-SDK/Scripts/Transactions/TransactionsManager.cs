using System.Threading.Tasks;
using LYNC.DeepLink;
using LYNC.Wallet;
using UnityEngine;

namespace LYNC.Transactions
{
    public class TransactionsManager
    {
        public async Task<TransactionResult> SendTransaction(Transaction transaction)
        {
            var tcs = new TaskCompletionSource<TransactionResult>();

            if (AuthBase.Instance is FirebaseAuth) // Server transactions
            {
                string url = ROUTES.GENERIC_TRANSACTION;
                LyncManager.Instance.StartCoroutine(API.CoroutineTransaction(url,transaction,
                txData =>
                {
                    LyncManager.Instance.SendTransactionAnalytics(AuthBase.Instance.accountAddress, txData.data.transactionHash.txHash, LyncManager.Instance.SponsorTransaction?"Gasless":"UserPaid");
                    tcs.SetResult(txData.ToTransactionResult());
                },
                err => 
                {
                    tcs.SetResult(err);
                }));
            }
            else if (AuthBase.Instance is StarKeyAuth) // Pontem
            {
                string transactionUrl;

                if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer) // Mobile Pontem
                {
                    // transaction.transactionId = DEEPLINK_MESSAGE_PATH.PONTEM_MOBILE_TRANSACTION;
                    // transactionUrl = await UrlBuilder.BuildPontemMobileTransactionUrlAsync(transaction);
                    transactionUrl = "";
                }
                else // Web Pontem
                {
                    // transaction.transactionId = System.Guid.NewGuid().ToString();
                    transactionUrl = UrlBuilder.BuildPontemBrowserTransactionUrl(transaction);
                }

                MessageHandler.AddListener<TransactionResult>(result =>
                {
                    tcs.SetResult(result);
                }, transaction);
                DeepLinkManager.Instance.StartBrowserProcess(transactionUrl);
            }
            else
            {
                throw new System.Exception("Unhandled");
            }

            return await tcs.Task;
        }
    }
}
