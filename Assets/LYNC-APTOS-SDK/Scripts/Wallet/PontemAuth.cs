using System;
using System.Threading.Tasks;
using UnityEngine;

public class PontemAuth : AuthBase
{
    public PontemAuth(string publicAddress)
    {
        PublicAddress = publicAddress;
        Save(this);
    }

    public PontemAuth()
    {
        PublicAddress = PlayerPrefs.GetString("_publicAddress", "");
        if (!string.IsNullOrEmpty(PublicAddress)) Save(this);
    }

    protected override void CustomeSave()
    {
        PlayerPrefs.SetString("_savedAuthType", AUTH_TYPE.PONTEM.ToString().ToLower());
    }

    protected override Task Load(Action onSessionExpired = null)
    {
        PublicAddress = PlayerPrefs.GetString("_savedAuthType", "");
        if (!string.IsNullOrEmpty(PublicAddress)) Save(this);
        return default;
    }

    protected override void CustomLogout()
    {

    }
}