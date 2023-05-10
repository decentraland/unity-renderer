using System;

namespace MainScripts.DCL.Controllers.FriendsController
{
    public interface IMatrixInitializationBridge
    {
        event Action<string> OnReceiveMatrixAccessToken;

        void InitializeMatrix(string json);
    }
}
