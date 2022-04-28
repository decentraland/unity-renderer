using DCL;
using DCL.HelpAndSupportHUD;
using DCL.Helpers;
using DCL.Huds.QuestsPanel;
using DCL.Huds.QuestsTracker;
using DCL.SettingsPanelHUD;
using SignupHUD;
using UnityEngine;

public class HUDFactory : IHUDFactory
{
    public virtual IHUD CreateHUD(HUDElementID hudElementId)
    {
        IHUD hudElement = null;
        switch (hudElementId)
        {
            case HUDElementID.NONE:
                break;
            case HUDElementID.MINIMAP:
                hudElement = new MinimapHUDController();
                break;
            case HUDElementID.PROFILE_HUD:
                hudElement = new ProfileHUDController(new UserProfileWebInterfaceBridge());
                break;
            case HUDElementID.NOTIFICATION:
                hudElement = new NotificationHUDController();
                break;
            case HUDElementID.AVATAR_EDITOR:
                hudElement = new AvatarEditorHUDController(DataStore.i.featureFlags);
                break;
            case HUDElementID.SETTINGS_PANEL:
                hudElement = new SettingsPanelHUDController();
                break;
            case HUDElementID.PLAYER_INFO_CARD:
                hudElement = new PlayerInfoCardHUDController(FriendsController.i,
                    Resources.Load<StringVariable>("CurrentPlayerInfoCardId"),
                    new UserProfileWebInterfaceBridge(),
                    new WearablesCatalogControllerBridge(),
                    ProfanityFilterSharedInstances.regexFilter,
                    DataStore.i);
                break;
            case HUDElementID.AIRDROPPING:
                hudElement = new AirdroppingHUDController();
                break;
            case HUDElementID.TERMS_OF_SERVICE:
                hudElement = new TermsOfServiceHUDController();
                break;
            case HUDElementID.FRIENDS:
                hudElement = new FriendsHUDController();
                break;
            case HUDElementID.WORLD_CHAT_WINDOW:
                hudElement = new WorldChatWindowController(
                    new UserProfileWebInterfaceBridge(),
                    FriendsController.i, ChatController.i,
                    Environment.i.serviceLocator.Get<ILastReadMessagesService>());
                break;
            case HUDElementID.PRIVATE_CHAT_WINDOW:
                hudElement = new PrivateChatWindowController(DataStore.i,
                    new UserProfileWebInterfaceBridge(),
                    ChatController.i, FriendsController.i,
                    Resources.Load<InputAction_Trigger>("CloseWindow"),
                    Environment.i.serviceLocator.Get<ILastReadMessagesService>());
                break;
            case HUDElementID.PUBLIC_CHAT_CHANNEL:
                hudElement = new PublicChatChannelController(ChatController.i,
                    Environment.i.serviceLocator.Get<ILastReadMessagesService>(),
                    new UserProfileWebInterfaceBridge(),
                    Resources.Load<InputAction_Trigger>("CloseWindow"),
                    DataStore.i,
                    ProfanityFilterSharedInstances.regexFilter);
                break;
            case HUDElementID.TASKBAR:
                hudElement = new TaskbarHUDController();
                break;
            case HUDElementID.OPEN_EXTERNAL_URL_PROMPT:
                hudElement = new ExternalUrlPromptHUDController();
                break;
            case HUDElementID.NFT_INFO_DIALOG:
                hudElement = new NFTPromptHUDController();
                break;
            case HUDElementID.TELEPORT_DIALOG:
                hudElement = new TeleportPromptHUDController();
                break;
            case HUDElementID.CONTROLS_HUD:
                hudElement = new ControlsHUDController();
                break;
            case HUDElementID.HELP_AND_SUPPORT_HUD:
                hudElement = new HelpAndSupportHUDController();
                break;
            case HUDElementID.USERS_AROUND_LIST_HUD:
                hudElement = new UsersAroundListHUDController();
                break;
            case HUDElementID.GRAPHIC_CARD_WARNING:
                hudElement = new GraphicCardWarningHUDController();
                break;
            case HUDElementID.BUILDER_IN_WORLD_MAIN:
                break;
            case HUDElementID.QUESTS_PANEL:
                hudElement = new QuestsPanelHUDController();
                break;
            case HUDElementID.QUESTS_TRACKER:
                hudElement = new QuestsTrackerHUDController();
                break;
            case HUDElementID.SIGNUP:
                hudElement = new SignupHUDController();
                break;
            case HUDElementID.BUILDER_PROJECTS_PANEL:
                break;
            case HUDElementID.LOADING:
                hudElement = new LoadingHUDController();
                break;
        }

        return hudElement;
    }

    public void Dispose()
    {
    }

    public void Initialize()
    {
    }
}