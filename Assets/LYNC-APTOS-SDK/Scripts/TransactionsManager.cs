using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LYNC.Transactions
{
    public class TransactionsManager
    {
        private BlockchainMiddleware middleware;

        public TransactionsManager(BlockchainMiddleware blockchainMiddleware)
        {
            middleware = blockchainMiddleware;
        }

        public async Task<TransactionData> SendTransaction(TRANSACTIONS transactionType, CustomTransaction customTransaction = null)
        {
            var tcs = new TaskCompletionSource<TransactionData>();
            middleware.SendTransaction(transactionType, customTransaction, txData => { tcs.SetResult(txData); }, err => { tcs.SetException(new Exception(err)); });

            return await tcs.Task;
        }


    }
}
