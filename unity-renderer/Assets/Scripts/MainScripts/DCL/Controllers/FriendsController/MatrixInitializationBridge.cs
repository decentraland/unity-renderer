using DCL.Social.Friends;
using JetBrains.Annotations;
using System;
using UnityEngine;

namespace MainScripts.DCL.Controllers.FriendsController
{
    public class MatrixInitializationBridge : MonoBehaviour
    {
        public event Action<string> OnReceiveMatrixAccessToken;

        public static MatrixInitializationBridge i { get; private set; }

        void Awake()
        {
            i = this;
        }

        [PublicAPI]
        public void InitializeMatrix(string json)
        {
            string token = JsonUtility.FromJson<MatrixInitializationMessage>(json).token;
            OnReceiveMatrixAccessToken?.Invoke(token);
        }
    }
}
