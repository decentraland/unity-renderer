using UnityEngine;

namespace DCL.LoadingScreen
{
    /// <summary>
    /// Display the current state of the Loading Screen Controller
    /// </summary>
    public class LoadingScreenView : MonoBehaviour, ILoadingScreenView
    {
        private const string PATH = "_LoadingScreen";

        public static LoadingScreenView Create() =>
            Instantiate(Resources.Load<LoadingScreenView>(PATH));

        public void Dispose() { }

        public void UpdateLoadingMessage() { }
    }
}
