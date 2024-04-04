using System;
using System.Threading.Tasks;
using UnityEngine;

public class KeylessAuth : AuthBase
{
    public string KeyPairPrivateKey;
    public string KeyPairPublicKey;
    public int ExpirationDateSeconds;

    public KeylessAuth(string accountAddress, string publicKey, string privateKey, int expirationDateSeconds)
    {
        PublicAddress = accountAddress;
        KeyPairPublicKey = publicKey;
        KeyPairPrivateKey = privateKey;
        ExpirationDateSeconds = expirationDateSeconds;

        Save(this);
    }
    public KeylessAuth() { }

    protected override void CustomeSave()
    {
        PlayerPrefs.SetString("keyless_publicKey", KeyPairPublicKey);
        PlayerPrefs.SetString("keyless_privateKey", KeyPairPrivateKey);
        PlayerPrefs.SetString("keyless_expirationDateSeconds", ExpirationDateSeconds.ToString());
        PlayerPrefs.SetString("_savedAuthType", AUTH_TYPE.KEYLESS.ToString().ToLower());
    }

    protected override Task Load(Action onSessionExpired = null)
    {
        try
        {
            string _publicKey = PlayerPrefs.GetString("keyless_publicKey", "");
            string _privateKey = PlayerPrefs.GetString("keyless_privateKey", "");
            string _expirationDateSeconds = PlayerPrefs.GetString("keyless_expirationDateSeconds", "0");
            string _accountAddress = PlayerPrefs.GetString("_publicAddress", "");
            DateTime.TryParse(PlayerPrefs.GetString("_loginDate", ""), out DateTime loadedLoginDate);
            LoginDate = loadedLoginDate;

            int expSecondsDifference = int.Parse(_expirationDateSeconds) - DateTime.Now.Second;
            if (expSecondsDifference < 0 || IsSessionExpired(expSecondsDifference))
            {
                Logout();
                Debug.Log("Session expired");
                onSessionExpired?.Invoke();
                return default;
            }

            PublicAddress = _accountAddress;
            KeyPairPublicKey = _publicKey;
            KeyPairPrivateKey = _privateKey;
            ExpirationDateSeconds = int.Parse(_expirationDateSeconds);

            Save(this, false);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }

        return default;
    }
}