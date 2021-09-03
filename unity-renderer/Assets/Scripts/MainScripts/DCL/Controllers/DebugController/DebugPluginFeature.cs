using DCL.Bots;
using DCL.Helpers;
using UnityEngine;

namespace DCL
{
    public class DebugPluginFeature : PluginFeature
    {
        private DebugController debugController;
        private DebugBridge debugBridge;
        private Promise<KernelConfigModel> kernelConfigPromise;

        public override void Initialize()
        {
            base.Initialize();
            SetupSystems();
            SetupKernelConfig();
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

        public override void Dispose()
        {
            KernelConfig.i.OnChange -= OnKernelConfigChanged;
            kernelConfigPromise?.Dispose();
            base.Dispose();
        }
    }
}