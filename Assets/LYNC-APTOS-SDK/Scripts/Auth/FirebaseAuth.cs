using System.Threading.Tasks;
using LYNC;
using UnityEngine;

public class FirebaseAuth : AuthBase
{
    public string FirebaseEmail, FirebaseUid;
    public AptosFirebaseAuthData AptosFirebaseAuthData = null;

    public FirebaseAuth() { }
    public FirebaseAuth(AptosFirebaseAuthData aptosWallet)
    {
        AptosFirebaseAuthData = aptosWallet;
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
        var tcs = new TaskCompletionSource<AptosFirebaseAuthData>();
        if (Instance != null && Instance is FirebaseAuth)
        {
            tcs.SetResult((Instance as FirebaseAuth).AptosFirebaseAuthData);
        }
        else
        {
            string firebaseEmail = PlayerPrefs.GetString("firebase_email", null);
            string firebaseUid = PlayerPrefs.GetString("firebase_firebaseUid", null);
            long.TryParse(PlayerPrefs.GetString("_loginDate", "0"), out long ticks);
            LoginDate = new System.DateTime(ticks);
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

            // Debug.Log("Fetching firebase data from server...");
            if (AptosFirebaseAuthData != null)
            {
                tcs.SetResult(AptosFirebaseAuthData);
                Save(this, false);
            }
            else
            {
                if (!string.IsNullOrEmpty(FirebaseEmail) && !string.IsNullOrEmpty(FirebaseUid))
                {
                    LyncManager.Instance.StartCoroutine(API.CoroutineGetFirebaseProfile(new AptosProfileScheme(FirebaseEmail, FirebaseUid),
                    wallet =>
                    {
                        AptosFirebaseAuthData = wallet;
                        Debug.Log(JsonUtility.ToJson(wallet));
                        PublicAddress = AptosFirebaseAuthData.publicKey;
                        tcs.SetResult(AptosFirebaseAuthData);
                        Save(this, false);
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
}