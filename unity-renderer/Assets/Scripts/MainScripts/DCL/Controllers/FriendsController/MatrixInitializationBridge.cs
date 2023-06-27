using DCL;
using DCL.Helpers;
using DCL.Social.Friends;
using JetBrains.Annotations;
using System;
using UnityEngine;

namespace MainScripts.DCL.Controllers.FriendsController
{

    public class MatrixInitializationBridge : MonoBehaviour, IMatrixInitializationBridge
    {
        public string AccessToken { get; private set; }

        public event Action<string> OnReceiveMatrixAccessToken;

        public static MatrixInitializationBridge GetOrCreate()
        {
            var bridgeObj = SceneReferences.i?.bridgeGameObject;

            return bridgeObj == null
                ? new GameObject("Bridges").AddComponent<MatrixInitializationBridge>()
                : bridgeObj.GetOrCreateComponent<MatrixInitializationBridge>();
        }

        [PublicAPI]
        public void InitializeMatrix(string json)
        {
            string token = JsonUtility.FromJson<MatrixInitializationMessage>(json).token;
            AccessToken = token;
            OnReceiveMatrixAccessToken?.Invoke(token);
        }
    }
}
