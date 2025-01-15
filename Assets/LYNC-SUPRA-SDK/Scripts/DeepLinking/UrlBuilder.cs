using System.Threading.Tasks;
using LYNC.Wallet;
using UnityEngine;

namespace LYNC.DeepLink
{
    public static class UrlBuilder
    {
        // public static string BuildPontemMobileAuthUrl()
        // {
            // PontemMobileAuthOutScheme outScheme = new PontemMobileAuthOutScheme();
            // string temp = "pontem-wallet://mob2mob?connect=" + outScheme.ToBase64();
            // Debug.Log("BuildPontemMobileAuthUrl = " + temp);
            // return temp;
        // }

        // public static async Task<string> BuildPontemMobileTransactionUrlAsync(Transaction transaction)
        // {
        //     var tcs = new TaskCompletionSource<string>();
        //     LyncManager.Instance.StartCoroutine(API.CouroutineBuildMobileTransaction(transaction, res => tcs.SetResult(res), err => tcs.SetException(new System.Exception(err))));
        //     try
        //     {
        //         string result = await tcs.Task;

                // PontemMobileAuthOutScheme appInfo = new PontemMobileAuthOutScheme();
                // GameObject.FindGameObjectWithTag("debug").GetComponent<TMPro.TMP_Text>().text += JsonUtility.ToJson(appInfo);

                // string temp = "pontem-wallet://mob2mob?payload=" + Utils.ToBase64(result) + "&app_info=" + appInfo.ToBase64();
                // Debug.Log(temp);
                // return temp;
        //     }
        //     catch (System.Exception e)
        //     {
        //         Debug.LogException(e);
        //         throw;
        //     }
        // }

        public static string BuildPontemBrowserTransactionUrl(Transaction transaction)
        {
            return $"{LyncManager.BaseFrontEndURL}/starkey-transaction?scheme={DeepLinkRegistration.DeepLinkUrl}&transaction={Utils.ToBase64(transaction.ToJson())}&network={LyncManager.Instance.Network.ToString()}";
        }

        public static string BuildBrowserAuthUrl()
        {
            return LyncManager.BaseServerURL + "/auth" + "?scheme=" + DeepLinkRegistration.DeepLinkUrl;
        }
    }
}