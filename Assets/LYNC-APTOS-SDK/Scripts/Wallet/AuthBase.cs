using System.Threading.Tasks;
using UnityEngine;

public enum AUTH_TYPE { FIREBASE, PONTEM }
public abstract class AuthBase
{
    public string PublicAddress = null;
    public System.DateTime LoginDate { protected set; get; }
    public static AUTH_TYPE AuthType;
    public static AuthBase Instance = null;

    public bool WalletConnected
    {
        private set { WalletConnected = value; }
        get => !string.IsNullOrEmpty(PublicAddress);
    }

    // Methods
    public void Save(AuthBase _Instance)
    {
        LoginDate = System.DateTime.Now;
        PlayerPrefs.SetString("_loginDate", LoginDate.ToString("yyyy-MM-dd HH:mm:ss"));
        PlayerPrefs.SetString("_publicAddress", PublicAddress);

        PlayerPrefs.Save();
        CustomeSave();
        Instance = _Instance;
    }

    public static void Logout()
    {
        PlayerPrefs.SetString("_loginDate", "");
        PlayerPrefs.SetString("_publicAddress", "");
        PlayerPrefs.SetString("_savedAuthType", "");
        PlayerPrefs.Save();
    }
    protected abstract void CustomLogout();

    protected abstract void CustomeSave();
    protected abstract Task Load(System.Action onSessionExpired = null);

    public static async Task<AuthBase> LoadSavedAuth()
    {
        AuthBase temp = null;
        switch (AuthType)
        {
            case AUTH_TYPE.FIREBASE:
                try
                {
                    temp = new FirebaseAuth();
                    await temp.Load();
                }
                catch (System.Exception e)
                {
                    Debug.LogError(e);
                }
                break;
            case AUTH_TYPE.PONTEM:
                temp = new PontemAuth();
                break;
            default:
                break;
        }

        return temp;
    }

    // TO-DO fetch the sessionInSeconds param from the general setting 
    public bool IsSessionExpired(int sessionInSeconds = 604800) // 604800 is 7 days
    {
        return System.DateTime.Now > LoginDate.AddSeconds(sessionInSeconds);
    }
}