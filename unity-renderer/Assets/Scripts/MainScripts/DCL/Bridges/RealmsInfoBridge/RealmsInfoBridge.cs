using DCL.Helpers;
using System;
using JetBrains.Annotations;
using UnityEngine;
using static DCL.Interface.WebInterface;

namespace DCL
{
    public class RealmsInfoBridge : MonoBehaviour
    {
        public event Action<JumpInPayload> OnRealmConnectionSuccess;
        public event Action<JumpInPayload> OnRealmConnectionFailed;

        private readonly RealmsInfoHandler handler = new RealmsInfoHandler();

        public void UpdateRealmsInfo(string payload) { handler.Set(payload); }

        public void ConnectionToRealmSuccess(string json)
        {
            var realmConnectionSuccessPayload = Utils.SafeFromJson<JumpInPayload>(json);
            OnRealmConnectionSuccess?.Invoke(realmConnectionSuccessPayload);
        }

        public void ConnectionToRealmFailed(string json)
        {
            var realmConnectionFailedPayload = Utils.SafeFromJson<JumpInPayload>(json);
            OnRealmConnectionFailed?.Invoke(realmConnectionFailedPayload);
        }

        [PublicAPI]
        public void SetRealmAbout(string payload)
        {
            handler.SetAbout(payload);
        }
    }
}
