using DCL;
using DCL.Helpers;
using DCL.LoadingScreen.V2;
using System.Collections.Generic;

public class LoadingScreenV2Plugin : IPlugin
{
    // FD:: before the merge
    private readonly DataStoreRef<DataStore_LoadingScreen> dataStoreLoadingScreen;

    // FD:: after the merge from dev
    private readonly LoadingScreenHintsController controller;

    public LoadingScreenV2Plugin()
    {
        // FD:: before the merge
        dataStoreLoadingScreen.Ref.decoupledLoadingHUD.loadingScreenV2Enabled.Set(true);

        // FD:: after the merge from dev
        // controller = new LoadingScreenHintsController(
        //     new HintRequestService(new List<IHintRequestSource>(),
        //         new SceneController(new PlayerPrefsConfirmedExperiencesRepository(new DefaultPlayerPrefs())), new HintTextureRequestHandler())
        // );
    }

    public void Dispose()
    {
        // FD:: after the merge from dev
        controller.Dispose();
    }
}
