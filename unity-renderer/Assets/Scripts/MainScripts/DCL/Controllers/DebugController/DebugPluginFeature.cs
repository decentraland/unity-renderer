using DCL.Bots;
using DCL.Helpers;
using UnityEngine;

namespace DCL
{
    public class DebugPluginFeature : PluginFeature
    {
        private DebugController debugController;
        private DebugBridge debugBridge;

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
            Promise<KernelConfigModel> configPromise = KernelConfig.i.EnsureConfigInitialized();
            configPromise.Catch(Debug.Log);
            configPromise.Then(OnKernelConfigChanged);
            KernelConfig.i.OnChange += (current, previous) => OnKernelConfigChanged(current);
        }
        private void OnKernelConfigChanged(KernelConfigModel kernelConfig)
        {
            var network = kernelConfig.network;
            var realm = DataStore.i.playerRealm.Get().serverName;
            debugController.ShowInfoPanel(network, realm);
        }
    }
}