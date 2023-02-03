using DCL;
using DCL.Controllers.HUD;
using MainScripts.DCL.Controllers.HUD.Profile;
using MainScripts.DCL.Controllers.HUD.SettingsPanelHUDDesktop.Scripts;
using SocialFeaturesAnalytics;

public class HUDDesktopFactory : HUDFactory
{
    public override IHUD CreateHUD(HUDElementID hudElementId)
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
                hudElement = new LoadingHUDControllerDesktop();
                break;
            default:
                hudElement = base.CreateHUD(hudElementId);
                break;
        }

        return hudElement;
    }
}
