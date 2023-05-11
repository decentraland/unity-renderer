using DCL.Helpers;
using DCL.Providers;
using System.Threading;
using UnityEngine;

namespace DCL.Chat.HUD
{
    public class PromoteChannelsToastPlugin : IPlugin
    {
        private readonly CancellationTokenSource cts = new ();

        private PromoteChannelsToastComponentController promoteChannelsToastController;

        public PromoteChannelsToastPlugin()
        {
            Initialize(cts.Token);
        }

        private async void Initialize(CancellationToken ct)
        {
            var view = await Environment.i.serviceLocator.Get<IAddressableResourceProvider>()
                                        .Instantiate<PromoteChannelsToastComponentView>("PromoteChannelsHUD", cancellationToken: ct);

            promoteChannelsToastController = new PromoteChannelsToastComponentController(
                view, new DefaultPlayerPrefs(), DataStore.i, CommonScriptableObjects.rendererState);
        }

        public void Dispose()
        {
            cts.Cancel();
            cts.Dispose();
            promoteChannelsToastController.Dispose();
        }
    }
}
