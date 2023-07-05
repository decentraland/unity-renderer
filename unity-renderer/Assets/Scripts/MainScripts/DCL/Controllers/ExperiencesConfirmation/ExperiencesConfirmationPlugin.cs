using Cysharp.Threading.Tasks;
using DCL.Providers;
using DCL.WorldRuntime.PortableExperiences;
using System.Threading;

namespace DCL.PortableExperiences.Confirmation
{
    public class ExperiencesConfirmationPlugin : IPlugin
    {
        private readonly CancellationTokenSource pluginCancellationToken = new ();
        private ExperiencesConfirmationPopupController popupController;
        private TriggerPortableExperienceController triggerController;

        public ExperiencesConfirmationPlugin()
        {
            Initialize(pluginCancellationToken.Token).Forget();
        }

        private async UniTaskVoid Initialize(CancellationToken cancellationToken)
        {
            var view = await Environment.i.serviceLocator
                                        .Get<IAddressableResourceProvider>()
                                        .Instantiate<ExperiencesConfirmationPopupComponentView>("ExperiencesConfirmationPopup",
                                             "ExperiencesConfirmationPopup", cancellationToken: cancellationToken);

            popupController = new ExperiencesConfirmationPopupController(view,
                DataStore.i);

            triggerController = new TriggerPortableExperienceController(DataStore.i,
                Environment.i.serviceLocator.Get<IPortableExperiencesController>(),
                () => Environment.i.world.state);
        }

        public void Dispose()
        {
            popupController.Dispose();
            triggerController.Dispose();
            pluginCancellationToken.Cancel();
            pluginCancellationToken.Dispose();
        }
    }
}
