using DCL.Bots;
using DCL.Helpers;
using UnityEngine;

namespace DCL
{
    public class DebugPluginFeature : IPlugin
    {
        private DebugController debugController;
        private DebugBridge debugBridge;
        private Promise<KernelConfigModel> kernelConfigPromise;

        public bool enabled { get; private set; } = false;

        public void Enable()
        {
            enabled = true;
            SetupSystems();
            SetupKernelConfig();
        }

        public void Disable()
        {
            enabled = false;
        }

        public void OnGUI()
        {
        }

        public void Update()
        {
        }

        public void LateUpdate()
        {
        }

        private void SetupSystems()
        {
            var botsController = new BotsController();
            debugController = new DebugController(botsController);
            debugBridge = GameObject.Find("Main").AddComponent<DebugBridge>(); // todo: unuglyfy this
            debugBridge.Setup(debugController);
        }

        private void SetupKernelConfig()
        {
            kernelConfigPromise = KernelConfig.i.EnsureConfigInitialized();
            kernelConfigPromise.Catch(Debug.Log);
            kernelConfigPromise.Then(OnKernelConfigChanged);
            KernelConfig.i.OnChange += OnKernelConfigChanged;
        }

        private void OnKernelConfigChanged(KernelConfigModel current, KernelConfigModel previous) { OnKernelConfigChanged(current); }

        private void OnKernelConfigChanged(KernelConfigModel kernelConfig)
        {
            var network = kernelConfig.network;
            if (IsInfoPanelVisible(network))
            {
                var realm = GetRealmName();
                debugController.ShowInfoPanel(network, realm);
            }
            else
            {
                debugController.HideInfoPanel();
            }
        }

        private bool IsInfoPanelVisible(string network)
        {
#if UNITY_EDITOR
            return true;
#endif
            return !network.ToLower().Contains("mainnet");
        }

        private static string GetRealmName() { return DataStore.i.playerRealm.Get()?.serverName; }

        public void Dispose()
        {
            KernelConfig.i.OnChange -= OnKernelConfigChanged;
            kernelConfigPromise?.Dispose();
        }
    }
}