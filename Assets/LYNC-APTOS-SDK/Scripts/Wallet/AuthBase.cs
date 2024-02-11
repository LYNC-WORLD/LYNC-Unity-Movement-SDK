using System.Threading.Tasks;
using UnityEngine;

public enum AUTH_TYPE { FIREBASE, PONTEM }
public abstract class AuthBase
{
    public string PublicAddress = null;
    public System.DateTime LoginDate { protected set; get; }
    public static AUTH_TYPE AuthType;

    public bool WalletConnected
    {
        private set { WalletConnected = value; }
        get => !string.IsNullOrEmpty(PublicAddress);
    }

    // Constructors
    public AuthBase() { }
    public AuthBase(AUTH_TYPE authType) { AuthType = authType; }


    // Methods
    public void Save()
    {
        LoginDate = System.DateTime.Now;
        PlayerPrefs.SetString("_loginDate", LoginDate.ToString("yyyy-MM-dd HH:mm:ss"));
        PlayerPrefs.SetString("_savedAuthType", AuthType.ToString().ToLower());

        PlayerPrefs.Save();
        CustomeSave();
        Debug.Log("Auth data saved successfully!");
    }

    public static void Logout()
    {
        PlayerPrefs.SetString("_loginDate", "");
        PlayerPrefs.Save();
    }

    // Abstract methods
    protected abstract void CustomeSave();
    public abstract Task Load(System.Action onSessionExpired = null);

    /// <summary>
    /// Used when the saved auth is unknown
    /// </summary>
    /// <returns></returns>
    public static AuthBase LoadSavedAuth()
    {
        switch (AuthType)
        {
            case AUTH_TYPE.FIREBASE:
                return new FirebaseAuth();
            case AUTH_TYPE.PONTEM:
                return new PontemAuth();
            default:
                return null;
        }
    }

    // TO-DO fetch the sessionInSeconds param from the general setting 
    public bool IsSessionExpired(int sessionInSeconds = 604800) // 604800 is 7 days
    {
        return System.DateTime.Now > LoginDate.AddSeconds(sessionInSeconds);
    }
}