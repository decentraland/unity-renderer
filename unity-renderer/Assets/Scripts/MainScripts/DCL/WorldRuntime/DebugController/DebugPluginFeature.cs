using DCL.Bots;
using DCL.Helpers;
using DCLServices.WearablesCatalogService;
using Decentraland.Bff;
using UnityEngine;
using Variables.RealmsInfo;

namespace DCL
{
    public class DebugPluginFeature : IPlugin
    {
        private DebugController debugController;
        private DebugBridge debugBridge;
        private Promise<KernelConfigModel> kernelConfigPromise;

        public DebugPluginFeature()
        {
            SetupSystems();
            SetupKernelConfig();
        }

        private void SetupSystems()
        {
            var botsController = new BotsController(Environment.i.serviceLocator.Get<IWearablesCatalogService>());
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
            DataStore.i.realm.realmName.OnChange += OnPlayerRealmChanged;
        }

        private void OnKernelConfigChanged(KernelConfigModel current, KernelConfigModel previous)
        {
            OnKernelConfigChanged(current);
        }

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

        private void OnPlayerRealmChanged(string current, string previous)
        {
            debugController.SetRealm(current);
        }

        private bool IsInfoPanelVisible(string network)
        {
#if UNITY_EDITOR
            return true;
#endif
            return !network.ToLower().Contains("mainnet");
        }

        private static string GetRealmName()
        {
            return DataStore.i.realm.playerRealm.Get()?.serverName;
        }

        public void Dispose()
        {
            KernelConfig.i.OnChange -= OnKernelConfigChanged;
            DataStore.i.realm.realmName.OnChange -= OnPlayerRealmChanged;
            kernelConfigPromise?.Dispose();
        }
    }
}
