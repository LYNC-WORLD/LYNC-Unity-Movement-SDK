using System;
using System.Threading.Tasks;
using UnityEngine;

public class StarKeyAuth : AuthBase
{
    public StarKeyAuth(string publicAddress)
    {
        PublicAddress = publicAddress;
        Save(this);
    }
    public StarKeyAuth()
    {
        PublicAddress = PlayerPrefs.GetString("_publicAddress", "");
        if (!string.IsNullOrEmpty(PublicAddress)) Save(this);
    }
    protected override void CustomeSave()
    {

    }

    protected override Task Load(Action onSessionExpired = null)
    {
        PublicAddress = PlayerPrefs.GetString("_savedAuthType", "");
        if (!string.IsNullOrEmpty(PublicAddress)) Save(this);
        return default;
    }
}