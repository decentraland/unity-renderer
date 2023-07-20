using System;

namespace MainScripts.DCL.Controllers.FriendsController
{
    public interface IMatrixInitializationBridge
    {
        public string AccessToken { get; }

        event Action<string> OnReceiveMatrixAccessToken;
    }
}
