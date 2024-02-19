using System.Threading.Tasks;
using UnityEngine;

namespace LYNC.Wallet
{
    // public class WalletData
    // {
    //     public string AptosEmail;
    //     public string AptosFirebaseUid;
    //     public System.DateTime loginDate { private set; get; }
    //     public AptosAuthData AptosAuthData = null;

    //     public static WalletData ConnectedWalletInstance;

    //     public bool WalletConnected
    //     {
    //         private set { WalletConnected = value; }
    //         get => !string.IsNullOrEmpty(AptosEmail) && !string.IsNullOrEmpty(AptosFirebaseUid);
    //     }

    //     public WalletData(AptosAuthData aptosWallet, bool save = true)
    //     {
    //         AptosEmail = aptosWallet.email;
    //         AptosFirebaseUid = aptosWallet.firebaseUid;
    //         AptosAuthData = aptosWallet;
    //         loginDate = System.DateTime.Now;
    //         if (save) Save();

    //         ConnectedWalletInstance = this;
    //     }

    //     public WalletData(AptosAuthData aptosWallet, System.DateTime loginDate, bool save = true)
    //     {
    //         AptosEmail = aptosWallet.email;
    //         AptosFirebaseUid = aptosWallet.firebaseUid;
    //         AptosAuthData = aptosWallet;
    //         this.loginDate = loginDate;
    //         if (save) Save();

    //         ConnectedWalletInstance = this;
    //         CheckSessionExpiration();
    //     }

    //     public WalletData() { }

    //     public void Save()
    //     {
    //         PlayerPrefs.SetString("aptos_email", AptosEmail);
    //         PlayerPrefs.SetString("aptos_firebaseUid", AptosFirebaseUid);
    //         PlayerPrefs.SetString("_loginDate", loginDate.ToString("yyyy-MM-dd HH:mm:ss"));

    //         PlayerPrefs.Save();
    //         Debug.Log("Wallet data saved successfully!");
    //     }

    //     public async Task Load()
    //     {
    //         string aptosEmail = PlayerPrefs.GetString("aptos_email", null);
    //         string aptosFirebaseUid = PlayerPrefs.GetString("aptos_firebaseUid", null);

    //         System.DateTime.TryParse(PlayerPrefs.GetString("_loginDate", ""), out System.DateTime loadedLoginDate);

    //         loginDate = loadedLoginDate;
    //         AptosEmail = aptosEmail;
    //         AptosFirebaseUid = aptosFirebaseUid;
    //         var tcs = new TaskCompletionSource<WalletData>();
    //         if (AptosAuthData != null)
    //         {
    //             ConnectedWalletInstance = this;
    //             tcs.SetResult(this);
    //         }
    //         else
    //         {
    //             if (!string.IsNullOrEmpty(aptosEmail) && !string.IsNullOrEmpty(aptosFirebaseUid))
    //             {
    //                 LyncManager.Instance.StartCoroutine(API.CoroutineGetAptosProfile(API.BackendUrl + "/api/users/profile", new AptosProfileData(aptosEmail, aptosFirebaseUid),
    //                async wallet =>
    //                 {
    //                     AptosAuthData = wallet;
    //                     try
    //                     {
    //                         await GetBalance();
    //                     }
    //                     catch (System.Exception e)
    //                     {
    //                         Debug.LogError(e);
    //                     }

    //                     ConnectedWalletInstance = this;
    //                     tcs.SetResult(this);
    //                 }, msg =>
    //                 {
    //                     Debug.LogError(msg);
    //                     tcs.SetException(new System.Exception(msg));
    //                 }));
    //             }
    //             else
    //             {
    //                 Debug.LogError("Unknown error");
    //                 tcs.SetException(new System.Exception("Unknown error"));
    //             }
    //         }
    //         await tcs.Task;
    //     }

    //     public async Task<float> GetBalance()
    //     {
    //         var tcs = new TaskCompletionSource<float>();
    //         LyncManager.Instance.StartCoroutine(API.CoroutineGetBalance(AptosAuthData, res =>
    //         {
    //             AptosAuthData.balance = res;
    //             tcs.SetResult(res);
    //         }, err =>
    //         {
    //             tcs.SetException(new System.Exception(err));
    //         }));

    //         return await tcs.Task;
    //     }

    //     public static async Task<WalletData> TryLoadSavedWallet()
    //     {
    //         var tcs = new TaskCompletionSource<WalletData>();

    //         string aptosEmail = PlayerPrefs.GetString("aptos_email", null);
    //         string aptosFirebaseUid = PlayerPrefs.GetString("aptos_firebaseUid", null);

    //         if (!string.IsNullOrEmpty(aptosEmail) && !string.IsNullOrEmpty(aptosFirebaseUid))
    //         {
    //             System.DateTime.TryParse(PlayerPrefs.GetString("_loginDate", ""), out System.DateTime loadedLoginDate);
    //             LyncManager.Instance.StartCoroutine(API.CoroutineGetAptosProfile("http://localhost:5000/api/users/profile", new AptosProfileData(aptosEmail, aptosFirebaseUid),
    //             async wallet =>
    //             {
    //                 var loadedWallet = new WalletData(wallet, loadedLoginDate, false);
    //                 try
    //                 {
    //                     await loadedWallet.GetBalance();
    //                 }
    //                 catch (System.Exception e)
    //                 {
    //                     Debug.LogError(e);
    //                 }

    //                 ConnectedWalletInstance = loadedWallet;
    //                 tcs.SetResult(loadedWallet);
    //             }, msg =>
    //             {
    //                 tcs.SetException(new System.Exception(msg));
    //             }));
    //         }
    //         else
    //         {
    //             tcs.SetResult(new WalletData());
    //         }

    //         return await tcs.Task;
    //     }

    //     public static void Logout()
    //     {
    //         PlayerPrefs.SetString("aptos_email", "");
    //         PlayerPrefs.SetString("aptos_firebaseUid", "");
    //         PlayerPrefs.SetString("_loginDate", "");
    //         PlayerPrefs.Save();
    //     }

    //     private void CheckSessionExpiration(int sessionInSeconds = 604800) // 604800 is 7 days
    //     {
    //         if (System.DateTime.Now > loginDate.AddSeconds(sessionInSeconds))
    //         {
    //             Logout();
    //             Debug.Log("Session expired.");
    //         }
    //     }
    // }

}