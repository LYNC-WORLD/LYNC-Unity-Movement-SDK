using System.Threading.Tasks;
using LYNC;
using UnityEngine;

public class FirebaseAuth : AuthBase
{
    public string FirebaseEmail, FirebaseUid;
    public SupraFirebaseAuthDetails supraFirebaseAuthDetails = null;

    public FirebaseAuth() { }
    public FirebaseAuth(SupraFirebaseAuthDetails aptosWallet)
    {
        supraFirebaseAuthDetails = aptosWallet;
        FirebaseEmail = aptosWallet.email;
        FirebaseUid = aptosWallet.firebaseUid;
        accountAddress = aptosWallet.accountAddress;
        // Debug.Log(accountAddress);
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
        var tcs = new TaskCompletionSource<SupraFirebaseAuthDetails>();
        if (Instance != null && Instance is FirebaseAuth)
        {
            tcs.SetResult((Instance as FirebaseAuth).supraFirebaseAuthDetails);
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
            if (supraFirebaseAuthDetails != null)
            {
                tcs.SetResult(supraFirebaseAuthDetails);
                Save(this, false);
            }
            else
            {
                // if (!string.IsNullOrEmpty(FirebaseEmail) && !string.IsNullOrEmpty(FirebaseUid))
                if (!string.IsNullOrEmpty(FirebaseEmail))
                {
                    LyncManager.Instance.StartCoroutine(API.CoroutineGetFirebaseProfile(new SupraProfileScheme(FirebaseEmail, FirebaseUid),
                    wallet =>
                    {
                        supraFirebaseAuthDetails = wallet;
                        // Debug.Log(JsonUtility.ToJson(wallet));
                        accountAddress = supraFirebaseAuthDetails.accountAddress;
                        tcs.SetResult(supraFirebaseAuthDetails);
                        Save(this, true);
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