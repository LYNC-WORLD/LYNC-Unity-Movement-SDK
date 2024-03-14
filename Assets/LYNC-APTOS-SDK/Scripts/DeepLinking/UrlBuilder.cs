using LYNC.Wallet;
using UnityEngine;

namespace LYNC.DeepLink
{
    public static class UrlBuilder
    {
        public static string BuildPontemMobileAuthUrl()
        {
            PontemMobileAuthOutScheme outScheme = new PontemMobileAuthOutScheme();
            string temp = "pontem-wallet://mob2mob?connect=" + outScheme.ToBase64();
            Debug.Log("BuildPontemMobileAuthUrl = " + temp);
            return temp;
        }

        public static string BuildPontemMobileTransactionUrl(Transaction transaction)
        {
            PontemMobileAuthOutScheme appInfo = new PontemMobileAuthOutScheme();
            PontemMobileTransactionOutScheme transactionPayload = new PontemMobileTransactionOutScheme(transaction);
            string temp = "pontem-wallet://mob2mob?payload=" + transactionPayload.ToBase64() + "&app_info=" + appInfo.ToBase64();
            Debug.Log("BuildPontemMobileTransactionUrl = " + temp);
            Debug.Log(JsonUtility.ToJson(transactionPayload));
            return temp;
        }

        public static string BuildPontemBrowserTransactionUrl(Transaction transaction)
        {
            return $"{LyncManager.BaseFrontEndURL}/pontem-transaction?scheme={DeepLinkRegistration.DeepLinkUrl}&transaction={Utils.ToBase64(transaction.ToJson())}&network={(int)LyncManager.Instance.Network}";
        }

        public static string BuildBrowserAuthUrl()
        {
            return LyncManager.BaseServerURL + "/auth" + "?scheme=" + DeepLinkRegistration.DeepLinkUrl;
        }
    }
}