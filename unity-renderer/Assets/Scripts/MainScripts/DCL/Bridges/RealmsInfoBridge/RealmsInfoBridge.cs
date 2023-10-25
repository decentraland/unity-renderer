using Cysharp.Threading.Tasks;
using DCL.Helpers;
using System;
using JetBrains.Annotations;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Variables.RealmsInfo;
using static DCL.Interface.WebInterface;

namespace DCL
{
    public class RealmsInfoBridge : MonoBehaviour, IRealmsInfoBridge
    {
        private static RealmsInfoBridge instance;
        public static RealmsInfoBridge i => instance;

        public event Action<JumpInPayload> OnRealmConnectionSuccess;
        public event Action<JumpInPayload> OnRealmConnectionFailed;

        private readonly RealmsInfoHandler handler = new RealmsInfoHandler();

        private void Awake()
        {
            if (!instance)
                instance = this;
        }

        private void OnDestroy()
        {
            if (instance == this)
                instance = null;
        }

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

        public UniTask<IReadOnlyList<RealmModel>> FetchRealmsInfo(CancellationToken cancellationToken) =>
            handler.FetchRealmsInfo(cancellationToken);
    }
}
