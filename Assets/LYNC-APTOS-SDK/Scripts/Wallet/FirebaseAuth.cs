using System.Threading.Tasks;
using LYNC;
using UnityEngine;

public class FirebaseAuth : AuthBase
{
    public string FirebaseEmail, FirebaseUid;
    public AptosAuthData AptosAuthData = null;

    public FirebaseAuth()
    {
    }

    public FirebaseAuth(AptosAuthData aptosWallet)
    {
        AptosAuthData = aptosWallet;
        FirebaseEmail = aptosWallet.email;
        FirebaseUid = aptosWallet.firebaseUid;
        PublicAddress = aptosWallet.publicKey;
        Save(this);
    }

    protected override void CustomeSave()
    {
        PlayerPrefs.SetString("firebase_email", FirebaseEmail);
        PlayerPrefs.SetString("firebase_firebaseUid", FirebaseUid);
        PlayerPrefs.SetString("_savedAuthType", AUTH_TYPE.FIREBASE.ToString().ToLower());
    }

    protected override async Task Load(System.Action onSessionExpired = null)
    {
        var tcs = new TaskCompletionSource<AptosAuthData>();
        if (Instance != null && Instance is FirebaseAuth)
        {
            tcs.SetResult((Instance as FirebaseAuth).AptosAuthData);
        }
        else
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
            if (AptosAuthData != null)
            {
                tcs.SetResult(AptosAuthData);
                Save(this);
            }
            else
            {
                if (!string.IsNullOrEmpty(FirebaseEmail) && !string.IsNullOrEmpty(FirebaseUid))
                {
                    LyncManager.Instance.StartCoroutine(API.CoroutineGetFirebaseProfile(new AptosProfileData(FirebaseEmail, FirebaseUid),
                    wallet =>
                    {
                        AptosAuthData = wallet;
                        Debug.Log(JsonUtility.ToJson(wallet));
                        PublicAddress = AptosAuthData.publicKey;
                        tcs.SetResult(AptosAuthData);
                        Save(this);
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
        }

        await tcs.Task;
    }

    protected override void CustomLogout()
    {
        PlayerPrefs.SetString("firebase_email", "");
        PlayerPrefs.SetString("firebase_firebaseUid", "");
    }


}