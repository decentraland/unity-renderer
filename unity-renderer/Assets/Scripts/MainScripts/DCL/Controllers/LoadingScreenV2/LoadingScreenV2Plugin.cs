using DCL;
using DCL.Controllers.LoadingScreenV2;
using System.Collections.Generic;
using UnityEngine;

public class LoadingScreenV2Plugin : IPlugin
{
    private readonly LoadingScreenHintsController controller;

    public LoadingScreenV2Plugin()
    {
        controller = new LoadingScreenHintsController(
            new HintRequestService(new List<IHintRequestSource>(),
                new SceneController(), new HintTextureRequestHandler())
            );
    }

    public void Dispose()
    {
        controller.Dispose();
    }
}
