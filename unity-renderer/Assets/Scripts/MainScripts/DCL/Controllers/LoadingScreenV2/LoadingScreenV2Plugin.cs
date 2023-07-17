using DCL;
using DCL.Controllers.LoadingScreenV2;
using DCL.Helpers;
using DCL.World.PortableExperiences;
using System.Collections.Generic;
using UnityEngine;

public class LoadingScreenV2Plugin : IPlugin
{
    private readonly LoadingScreenHintsController controller;

    public LoadingScreenV2Plugin()
    {
        controller = new LoadingScreenHintsController(
            new HintRequestService(new List<IHintRequestSource>(),
                new SceneController(new PlayerPrefsConfirmedExperiencesRepository(new DefaultPlayerPrefs())), new HintTextureRequestHandler())
            );
    }

    public void Dispose()
    {
        controller.Dispose();
    }
}
