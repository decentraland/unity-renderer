using Cysharp.Threading.Tasks;
using DCL.Providers;
using System.Threading;

namespace DCL.PortableExperiences.Confirmation
{
    public class ExperiencesConfirmationPlugin : IPlugin
    {
        private readonly CancellationTokenSource pluginCancellationToken = new ();
        private ExperiencesConfirmationPopupController controller;

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

            controller = new ExperiencesConfirmationPopupController(view,
                DataStore.i);
        }

        public void Dispose()
        {
            controller.Dispose();
            pluginCancellationToken.Cancel();
            pluginCancellationToken.Dispose();
        }
    }
}
