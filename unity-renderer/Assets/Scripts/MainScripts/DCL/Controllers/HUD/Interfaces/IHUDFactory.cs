using Cysharp.Threading.Tasks;
using System;
using System.Threading;

namespace DCL
{
    public enum HUDElementID
    {
        NONE = 0,
        MINIMAP = 1,
        PROFILE_HUD = 2,
        NOTIFICATION = 3,
        [Obsolete("Deprecated. Migrated to plugins. See AvatarEditorHUDPlugin, BackpackEditorV2Plugin")]
        AVATAR_EDITOR = 4,
        SETTINGS_PANEL = 5,

        [Obsolete("Deprecated behavior")]
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
        [Obsolete("Deprecated behavior")]
        TELEPORT_DIALOG = 17,
        CONTROLS_HUD = 18,

        HELP_AND_SUPPORT_HUD = 20,

        USERS_AROUND_LIST_HUD = 22,
        GRAPHIC_CARD_WARNING = 23,

        [Obsolete("Deprecated HUD Element")]
        QUESTS_PANEL = 26,
        [Obsolete("Deprecated behavior")]
        QUESTS_TRACKER = 27,

        [Obsolete("Deprecated HUD Element, ported to Plugin System")]
        SIGNUP = 29,
        [Obsolete("Deprecated HUD Element")]
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
    }

    public interface IHUDFactory : IService
    {
        UniTask<IHUD> CreateHUD(HUDElementID elementID, CancellationToken cancellationToken = default);

        UniTask<T> CreateHUDView<T>(string assetAddress, CancellationToken cancellationToken = default, string name = null) where T:IDisposable;
    }
}
