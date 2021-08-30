using DCL.Bots;
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
            var botsController = new BotsController();
            debugController = new DebugController(botsController);
            debugBridge = GameObject.Find("Main").AddComponent<DebugBridge>(); // todo: unuglyfy this
            debugBridge.Setup(debugController);
        }
    }
}