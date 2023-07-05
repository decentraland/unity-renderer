using Cysharp.Threading.Tasks;
using DCL.Providers;
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
                DataStore.i);
        }

        public void Dispose()
        {
            popupController.Dispose();
            pluginCancellationToken.Cancel();
            pluginCancellationToken.Dispose();
        }
    }
}
