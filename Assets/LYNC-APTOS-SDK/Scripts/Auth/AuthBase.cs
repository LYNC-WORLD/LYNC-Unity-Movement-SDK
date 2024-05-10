using System;
using System.Threading.Tasks;
using UnityEngine;

public enum AUTH_TYPE { FIREBASE, PONTEM, KEYLESS }
public abstract class AuthBase
{
    public string PublicAddress = null;
    public DateTime LoginDate { protected set; get; }
    public static AUTH_TYPE AuthType;
    public static AuthBase Instance = null;

    public bool WalletConnected
    {
        private set { WalletConnected = value; }
        get => !string.IsNullOrEmpty(PublicAddress);
    }

    // Methods
    public void Save(AuthBase _Instance, bool insertDateNow = true)
    {
        if (insertDateNow)
            LoginDate = DateTime.Now;

        PlayerPrefs.SetString("_loginDate", LoginDate.Ticks.ToString());
        PlayerPrefs.SetString("_publicAddress", PublicAddress);

        CustomeSave();
        PlayerPrefs.Save();
        Instance = _Instance;
    }

    public static void Logout()
    {
        PlayerPrefs.SetString("_loginDate", "");
        PlayerPrefs.SetString("_publicAddress", "");
        PlayerPrefs.SetString("_savedAuthType", "");
        PlayerPrefs.SetString("firebase_email", "");
        PlayerPrefs.SetString("firebase_firebaseUid", "");

        PlayerPrefs.SetString("keyless_accountAddress", "");
        PlayerPrefs.SetString("keyless_privateKey", "");
        PlayerPrefs.SetString("keyless_expirationDateSeconds", "");

        PlayerPrefs.Save();
        Instance = null;
    }

    protected abstract void CustomeSave();
    protected abstract Task Load(Action onSessionExpired = null);

    public static async Task<AuthBase> LoadSavedAuth(Action onSessionExpired = null)
    {
        AuthBase temp = null;
        try
        {
            switch (AuthType)
            {
                case AUTH_TYPE.FIREBASE:
                    temp = new FirebaseAuth();
                    await temp.Load(onSessionExpired);
                    break;
                case AUTH_TYPE.PONTEM:
                    temp = new PontemAuth();
                    break;
                case AUTH_TYPE.KEYLESS:

                    temp = new KeylessAuth();
                    temp.Load(onSessionExpired);
                    break;
                default:
                    break;
            }

            return temp;
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            throw e;
        }
    }

    // TO-DO fetch the sessionInSeconds param from the general setting 
    public bool IsSessionExpired(long sessionInSeconds = 604800) // 604800 is 7 days
    {
        DateTime expirationDate = LoginDate.AddSeconds(sessionInSeconds);
        return DateTime.Now > expirationDate;
    }
}