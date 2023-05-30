using DCL;
using DCL.Controllers.LoadingScreenV2;
using System.Collections.Generic;

public class LoadingScreenV2Plugin : IPlugin
{
    private readonly HintRequestService controller;

    public LoadingScreenV2Plugin()
    {
        controller = new HintRequestService(new List<IHintRequestSource>(), new SceneController(), new HintTextureRequestHandler());
    }

    public void Dispose()
    {
        controller.Dispose();
    }
}
