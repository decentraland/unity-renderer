using Cysharp.Threading.Tasks;
using DCL.Providers;
using Login;
using System.Collections;
using UnityEngine;
using Environment = DCL.Environment;

public class LoginSceneController : MonoBehaviour
{
    private LoginHUDController _loginHUDController;

    // Start is called before the first frame update
    private IEnumerator Start()
    {
        var assetProvider = Environment.i.serviceLocator.Get<IAddressableResourceProvider>();

        yield return assetProvider.Instantiate<ILoginHUDView>("LoginHUD").ToCoroutine(ResultHandler);

        void ResultHandler(ILoginHUDView view)
        {
            _loginHUDController = new LoginHUDController(view);
            _loginHUDController.Initialize();
        }
    }
}
