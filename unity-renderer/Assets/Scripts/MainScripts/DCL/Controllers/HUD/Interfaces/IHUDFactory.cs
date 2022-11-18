using System;

namespace DCL
{
    public enum HUDElementID
    {
        NONE = 0,
        MINIMAP = 1,
        PROFILE_HUD = 2,
        NOTIFICATION = 3,
        AVATAR_EDITOR = 4,
        SETTINGS_PANEL = 5,

        [Obsolete("Deprecated HUD Element, replaced by EMOTES")]
        EXPRESSIONS = 6,

        PLAYER_INFO_CARD = 7,
        AIRDROPPING = 8,
        TERMS_OF_SERVICE = 9,
        WORLD_CHAT_WINDOW = 10,
        TASKBAR = 11,

        [Obsolete("Deprecated behavior")]
        MESSAGE_OF_THE_DAY = 12,
        
        FRIENDS = 13,
        OPEN_EXTERNAL_URL_PROMPT = 14,
        PRIVATE_CHAT_WINDOW = 15,
        NFT_INFO_DIALOG = 16,
        TELEPORT_DIALOG = 17,
        CONTROLS_HUD = 18,

        [Obsolete("Deprecated HUD Element")]
        EXPLORE_HUD = 19,

        HELP_AND_SUPPORT_HUD = 20,

        [Obsolete("Deprecated HUD Element")]
        EMAIL_PROMPT = 21,

        USERS_AROUND_LIST_HUD = 22,
        GRAPHIC_CARD_WARNING = 23,

        [Obsolete("Deprecated HUD Element")]
        BUILDER_IN_WORLD_MAIN = 24,

        [Obsolete("Deprecated HUD Element")]
        BUILDER_IN_WOLRD_INITIAL_PANEL = 25,

        QUESTS_PANEL = 26,
        QUESTS_TRACKER = 27,

        [Obsolete("Deprecated HUD Element")]
        BUILDER_PROJECTS_PANEL = 28,
        SIGNUP = 29,
        LOADING = 30,

        [Obsolete("Deprecated HUD Element")]
        AVATAR_NAMES = 31,

        [Obsolete("Deprecated HUD Element, this feature is initialized from the Feature Flags system")]
        EMOTES = 32,
        PUBLIC_CHAT = 33,
        CHANNELS_CHAT = 34,
        CHANNELS_SEARCH = 35,
        CHANNELS_CREATE = 36,
        CHANNELS_LEAVE_CONFIRMATION = 37,

        COUNT = 38
    }

    public interface IHUDFactory : IService
    {
        IHUD CreateHUD(HUDElementID elementID);
    }
}