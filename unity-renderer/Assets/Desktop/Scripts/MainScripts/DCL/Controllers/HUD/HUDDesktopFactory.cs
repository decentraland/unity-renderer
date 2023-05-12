using Cysharp.Threading.Tasks;
using DCL;
using DCL.Controllers.HUD;
using MainScripts.DCL.Controllers.HUD.Profile;
using MainScripts.DCL.Controllers.HUD.SettingsPanelHUDDesktop.Scripts;
using SocialFeaturesAnalytics;
using System.Threading;

public class HUDDesktopFactory : HUDFactory
{
    private const string LOADING_HUD_ADDRESS = "LoadingHUDDesktop";

    public override async UniTask<IHUD> CreateHUD(HUDElementID hudElementId, CancellationToken cancellationToken = default)
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
                        new UserProfileWebInterfaceBridge()),
                    DataStore.i);
                break;
            case HUDElementID.MINIMAP:
                hudElement = new MinimapHUDControllerDesktop(MinimapMetadataController.i, new WebInterfaceHomeLocationController(), DCL.Environment.i);
                break;

            default:
                hudElement = await base.CreateHUD(hudElementId, cancellationToken);
                break;
        }

        return hudElement;
    }
}
