using Cysharp.Threading.Tasks;
using DCL;
using DCL.Browser;
using DCL.Controllers.HUD;
using DCL.MyAccount;
using DCL.SettingsCommon;
using MainScripts.DCL.Controllers.HUD.Profile;
using MainScripts.DCL.Controllers.HUD.SettingsPanelHUDDesktop.Scripts;
using SocialFeaturesAnalytics;
using System.Threading;
using UnityEngine;

public class HUDDesktopFactory : HUDFactory
{
    private const string VIEW_NAME = "_ProfileHUD";

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
                ProfileHUDViewDesktop_V2 view = Object.Instantiate(Resources.Load<ProfileHUDViewDesktop_V2>("ProfileHUDDesktop_V2"));
                view.name = VIEW_NAME;

                var userProfileWebInterfaceBridge = new UserProfileWebInterfaceBridge();
                var webInterfaceBrowserBridge = new WebInterfaceBrowserBridge();

                hudElement = new ProfileHUDControllerDesktop(
                    view,
                    userProfileWebInterfaceBridge,
                    new SocialAnalytics(
                        Environment.i.platform.serviceProviders.analytics,
                        userProfileWebInterfaceBridge),
                    DataStore.i,
                    new MyAccountCardController(
                        view.MyAccountCardView,
                        DataStore.i,
                        userProfileWebInterfaceBridge,
                        Settings.i,
                        webInterfaceBrowserBridge),
                    webInterfaceBrowserBridge);
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
