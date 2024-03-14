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

    }

    protected override Task Load(Action onSessionExpired = null)
    {
        PublicAddress = PlayerPrefs.GetString("_savedAuthType", "");
        if (!string.IsNullOrEmpty(PublicAddress)) Save(this);
        return default;
    }
}