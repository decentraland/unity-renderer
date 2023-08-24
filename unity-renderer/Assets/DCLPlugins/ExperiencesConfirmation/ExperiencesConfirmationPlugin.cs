using Cysharp.Threading.Tasks;
using DCL.Helpers;
using DCL.Providers;
using DCL.World.PortableExperiences;
using DCLServices.PortableExperiences.Analytics;
using System.Threading;

namespace DCL.PortableExperiences.Confirmation
{
    public class ExperiencesConfirmationPlugin : IPlugin
    {
        private readonly CancellationTokenSource pluginCancellationToken = new ();
        private ExperiencesConfirmationPopupController popupController;

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
                DataStore.i,
                new PlayerPrefsConfirmedExperiencesRepository(new DefaultPlayerPrefs()),
                new UserProfileWebInterfaceBridge(),
                Environment.i.serviceLocator.Get<IPortableExperiencesAnalyticsService>());
        }

        public void Dispose()
        {
            popupController.Dispose();
            pluginCancellationToken.Cancel();
            pluginCancellationToken.Dispose();
        }
    }
}
