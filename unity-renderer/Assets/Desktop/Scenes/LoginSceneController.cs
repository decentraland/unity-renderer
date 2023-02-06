using Cysharp.Threading.Tasks;
using DCL.Providers;
using Login;
using System;
using System.Collections;
using System.Threading;
using UnityEngine;
using Environment = DCL.Environment;

public class LoginSceneController : MonoBehaviour
{
    private LoginHUDController _loginHUDController;

    private CancellationTokenSource cancellationTokenSource;

    // Start is called before the first frame update
    private IEnumerator Start()
    {
        var assetProvider = Environment.i.serviceLocator.Get<IAddressableResourceProvider>();
        cancellationTokenSource = new CancellationTokenSource();

        yield return assetProvider.Instantiate<ILoginHUDView>("LoginHUD", cancellationToken: cancellationTokenSource.Token)
                                  .ToCoroutine(ResultHandler);

        void ResultHandler(ILoginHUDView view)
        {
            _loginHUDController = new LoginHUDController(view);
            _loginHUDController.Initialize();
        }
    }

    private void OnDestroy()
    {
        cancellationTokenSource.Cancel();
        cancellationTokenSource.Dispose();
    }
}
