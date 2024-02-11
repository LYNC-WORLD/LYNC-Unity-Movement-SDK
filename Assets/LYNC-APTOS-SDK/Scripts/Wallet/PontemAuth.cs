using System;
using System.Threading.Tasks;

public class PontemAuth : AuthBase
{
    protected override void CustomeSave()
    {
        throw new NotImplementedException();
    }

    public override Task Load(Action onSessionExpired = null)
    {
        throw new NotImplementedException();
    }

    protected static void CustomLogout()
    {

    }
}