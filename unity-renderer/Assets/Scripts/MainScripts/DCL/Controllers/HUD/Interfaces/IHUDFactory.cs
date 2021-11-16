using System;

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
    MESSAGE_OF_THE_DAY = 12,
    FRIENDS = 13,
    OPEN_EXTERNAL_URL_PROMPT = 14,
    PRIVATE_CHAT_WINDOW = 15,
    NFT_INFO_DIALOG = 16,
    TELEPORT_DIALOG = 17,
    CONTROLS_HUD = 18,
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

    EMOTES = 32,

    COUNT = 33
}

public interface IHUDFactory
{
    IHUD CreateHUD(HUDElementID elementID);
}