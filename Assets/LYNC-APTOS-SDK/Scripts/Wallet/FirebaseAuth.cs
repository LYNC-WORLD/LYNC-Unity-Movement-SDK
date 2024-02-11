using System.Threading.Tasks;
using LYNC;
using UnityEngine;

public class FirebaseAuth : AuthBase
{
    public string FirebaseEmail, FirebaseUid;
    public AptosWallet AptosWallet = null;
    public static FirebaseAuth Instance = null;

    public FirebaseAuth()
    {
    }

    public FirebaseAuth(AptosWallet aptosWallet)
    {
        AptosWallet = aptosWallet;
        FirebaseEmail = aptosWallet.email;
        FirebaseUid = aptosWallet.firebaseUid;
        Save();

        Instance = this;
    }

    protected override void CustomeSave()
    {
        PlayerPrefs.SetString("firebase_email", FirebaseEmail);
        PlayerPrefs.SetString("firebase_firebaseUid", FirebaseUid);
    }

    public override async Task Load(System.Action onSessionExpired = null)
    {
        string firebaseEmail = PlayerPrefs.GetString("firebase_email", null);
        string firebaseUid = PlayerPrefs.GetString("firebase_firebaseUid", null);
        System.DateTime.TryParse(PlayerPrefs.GetString("_loginDate", ""), out System.DateTime loadedLoginDate);
        LoginDate = loadedLoginDate;
        FirebaseEmail = firebaseEmail;
        FirebaseUid = firebaseUid;

        if (string.IsNullOrEmpty(firebaseEmail) || string.IsNullOrEmpty(firebaseUid)) return;

        if (IsSessionExpired())
        {
            Logout();
            Debug.Log("Session expired");
            onSessionExpired?.Invoke();
            return;
        }
        Debug.Log("Fetching firebase data from server...");
        var tcs = new TaskCompletionSource<AptosWallet>();
        if (AptosWallet != null)
        {
            tcs.SetResult(AptosWallet);
            Instance = this;
        }
        else
        {
            if (!string.IsNullOrEmpty(FirebaseEmail) && !string.IsNullOrEmpty(FirebaseUid))
            {
                LyncManager.Instance.StartCoroutine(API.CoroutineGetFirebaseProfile(API.BackendUrl + "/api/users/profile", new AptosProfileData(FirebaseEmail, FirebaseUid),
               async wallet =>
                {
                    AptosWallet = wallet;
                    Debug.Log(JsonUtility.ToJson(wallet));
                    try
                    {
                        await AptosWallet.UpdateBalance();
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError(e);
                    }

                    Instance = this;
                    PublicAddress = AptosWallet.publicKey;
                    tcs.SetResult(AptosWallet);
                }, msg =>
                {
                    Debug.LogError(msg);
                    tcs.SetException(new System.Exception(msg));
                }));
            }
            else
            {
                Debug.LogError("Unknown error when loading firebase auth");
                tcs.SetException(new System.Exception("Unknown error when loading firebase auth"));
            }
        }

        await tcs.Task;
    }

    protected static void CustomLogout()
    {
        PlayerPrefs.SetString("firebase_email", "");
        PlayerPrefs.SetString("firebase_firebaseUid", "");
    }
}