using Cysharp.Threading.Tasks;
using DCL.Providers;
using Login;
using System.Collections;
using System.Threading;
using UnityEngine;
using Environment = DCL.Environment;

namespace Desktop.Scenes
{
    public class LoginSceneController : MonoBehaviour
    {
        private const string LOGIN_HUD_ADDRESS = "LoginHUD";
        private CancellationTokenSource cancellationTokenSource;

        private void Start()
        {
            cancellationTokenSource = new CancellationTokenSource();
            CreateHUD(Environment.i.serviceLocator.Get<IAddressableResourceProvider>(), cancellationTokenSource.Token).Forget();
        }

        private static async UniTaskVoid CreateHUD(IAddressableResourceProvider assetProvider, CancellationToken tokenSource)
        {
            ILoginHUDView view = await assetProvider.Instantiate<ILoginHUDView>(LOGIN_HUD_ADDRESS, cancellationToken: tokenSource);
            new LoginHUDController(view).Initialize();
        }

        private void OnDestroy()
        {
            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();
        }
    }
}
