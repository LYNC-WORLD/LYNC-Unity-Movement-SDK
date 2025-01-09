using System;
using System.Threading.Tasks;
using UnityEngine;

public class KeylessAuth : AuthBase
{
    public string KeyPairPrivateKey;
    public string KeyPairPublicKey;
    public string dataId;
    public int ExpirationDateSeconds;

    public KeylessAuth() { }
    public KeylessAuth(string accountAddress, string publicKey, string privateKey, int expirationDateSeconds, string dataId)
    {
        base.accountAddress = accountAddress;
        KeyPairPublicKey = publicKey;
        KeyPairPrivateKey = privateKey;
        ExpirationDateSeconds = expirationDateSeconds;
        this.dataId = dataId;

        Save(this);
    }

    protected override void CustomeSave()
    {
        PlayerPrefs.SetString("keyless_publicKey", KeyPairPublicKey);
        PlayerPrefs.SetString("keyless_privateKey", KeyPairPrivateKey);
        PlayerPrefs.SetString("keyless_expirationDateSeconds", ExpirationDateSeconds.ToString());
        PlayerPrefs.SetString("keyless_jwt", dataId);
        // PlayerPrefs.SetString("_savedAuthType", AUTH_TYPE.KEYLESS.ToString().ToLower());
    }

    protected override Task Load(Action onSessionExpired = null)
    {
        try
        {
            string _publicKey = PlayerPrefs.GetString("keyless_publicKey", "");
            string _privateKey = PlayerPrefs.GetString("keyless_privateKey", "");
            string _dataId = PlayerPrefs.GetString("keyless_jwt", "");
            string _expirationDateSeconds = PlayerPrefs.GetString("keyless_expirationDateSeconds", "0");
            string _accountAddress = PlayerPrefs.GetString("_publicAddress", "");
            long.TryParse(PlayerPrefs.GetString("_loginDate", "0"), out long ticks);
            LoginDate = new DateTime(ticks);

            long expSecondsDifference = long.Parse(_expirationDateSeconds) - DateTimeOffset.Now.ToUnixTimeSeconds();
            if (IsSessionExpired(expSecondsDifference))
            {
                onSessionExpired?.Invoke();
                Logout();
                Debug.Log("session expired");
                return default;
            }

            accountAddress = _accountAddress;
            KeyPairPublicKey = _publicKey;
            KeyPairPrivateKey = _privateKey;
            ExpirationDateSeconds = int.Parse(_expirationDateSeconds);
            dataId = _dataId;

            Save(this, false);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }

        return default;
    }
}