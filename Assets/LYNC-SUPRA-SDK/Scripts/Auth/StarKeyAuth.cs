using System;
using System.Threading.Tasks;
using UnityEngine;

public class StarKeyAuth : AuthBase
{
    public StarKeyAuth(string publicAddress)
    {
        accountAddress = publicAddress;
        Save(this);
    }
    public StarKeyAuth()
    {
        accountAddress = PlayerPrefs.GetString("_publicAddress", "");
        if (!string.IsNullOrEmpty(accountAddress)) Save(this);
    }
    protected override void CustomeSave()
    {

    }

    protected override Task Load(Action onSessionExpired = null)
    {
        accountAddress = PlayerPrefs.GetString("_savedAuthType", "");
        if (!string.IsNullOrEmpty(accountAddress)) Save(this);
        return default;
    }
}