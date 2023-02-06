using Cysharp.Threading.Tasks;
using DCL;
using DCL.Controllers.HUD;
using DCL.Providers;
using MainScripts.DCL.Controllers.HUD.Profile;
using MainScripts.DCL.Controllers.HUD.SettingsPanelHUDDesktop.Scripts;
using SocialFeaturesAnalytics;

public class HUDDesktopFactory : HUDFactory
{
    private const string LOADING_HUD_ADDRESS = "LoadingHUDDesktop";

    public HUDDesktopFactory(IAddressableResourceProvider assetsProvider) : base(assetsProvider)
    {
    }

    public override async UniTask<IHUD> CreateHUD(HUDElementID hudElementId)
    {
        IHUD hudElement = null;

        switch (hudElementId)
        {
            case HUDElementID.NONE:
                break;
            case HUDElementID.SETTINGS_PANEL:
                hudElement = new SettingsPanelHUDControllerDesktop();
                break;
            case HUDElementID.PROFILE_HUD:
                hudElement = new ProfileHUDControllerDesktop(new UserProfileWebInterfaceBridge(),
                    new SocialAnalytics(
                        Environment.i.platform.serviceProviders.analytics,
                        new UserProfileWebInterfaceBridge()));
                break;
            case HUDElementID.MINIMAP:
                hudElement = new MinimapHUDControllerDesktop(MinimapMetadataController.i, new WebInterfaceHomeLocationController(), DCL.Environment.i);
                break;

            case HUDElementID.LOADING:
                var loadingHUDView = await CreateHUDView<LoadingHUDView>(LOADING_HUD_ADDRESS);
                loadingHUDView.Initialize();
                return new LoadingHUDController(loadingHUDView);

            default:
                hudElement = await base.CreateHUD(hudElementId);
                break;
        }

        return hudElement;
    }
}
