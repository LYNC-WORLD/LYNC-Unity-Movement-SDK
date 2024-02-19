using System.Threading.Tasks;
using LYNC.DeepLink;
using LYNC.Wallet;

namespace LYNC.Transactions
{
    public class TransactionsManager
    {
        public Task<TransactionResult> SendTransaction(Transaction transaction)
        {
            var tcs = new TaskCompletionSource<TransactionResult>();
            if (AuthBase.Instance is PontemAuth)
                UnityEngine.Debug.Log("PontemAuth");
            if (AuthBase.Instance is FirebaseAuth)
                UnityEngine.Debug.Log("FirebaseAuth");

            if (AuthBase.Instance is FirebaseAuth)
            {
                LyncManager.Instance.StartCoroutine(API.CoroutineTransaction(transaction, txData => { tcs.SetResult(txData.ToTransactionResult()); }, err => { tcs.SetException(new System.Exception(err)); }));
            }
            else if (AuthBase.Instance is PontemAuth)
            {
                transaction.transactionId = System.Guid.NewGuid().ToString();
                MessageHandler.AddTransactionListener(transaction, result =>
                {
                    tcs.SetResult(result);
                });
                DeepLinkManager.Instance.StartBrowserProcess(transaction.GetBrowserUrl());
            }
            else
            {
                throw new System.Exception("Unhandled");
            }


            return tcs.Task;
        }
    }
}
