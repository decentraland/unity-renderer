using DCL.Components.Interfaces;
using System.Collections.Generic;
using UnityEngine;

namespace DCL
{
    public class DataStore_HUDs
    {
        public readonly BaseVariable<bool> connectWalletModalVisible = new BaseVariable<bool>(false);
        public readonly BaseVariable<bool> closedWalletModal = new BaseVariable<bool>(false);
        public readonly BaseVariable<bool> questsPanelVisible = new BaseVariable<bool>(false);
        public readonly BaseVariable<bool> builderProjectsPanelVisible = new BaseVariable<bool>(false);
        public readonly BaseVariable<bool> signupVisible = new BaseVariable<bool>(false);
        public readonly BaseVariable<bool> controlsVisible = new BaseVariable<bool>(false);
        public readonly BaseVariable<bool> isAvatarEditorInitialized = new BaseVariable<bool>(false);
        public readonly BaseVariable<bool> avatarEditorVisible = new BaseVariable<bool>(false);
        public readonly BaseVariable<bool> emotesVisible = new BaseVariable<bool>(false);
        public readonly BaseVariable<bool> emoteJustTriggeredFromShortcut = new BaseVariable<bool>(false);
        public readonly BaseVariable<bool> isNavMapInitialized = new BaseVariable<bool>(false);
        public readonly BaseVariable<bool> navmapVisible = new BaseVariable<bool>(false);
        public readonly BaseVariable<bool> navmapIsRendered = new BaseVariable<bool>(false);
        public readonly BaseVariable<Texture> mapMainTexture = new BaseVariable<Texture>(null);
        public readonly BaseVariable<Texture> mapEstatesTexture = new BaseVariable<Texture>(null);
        public readonly BaseVariable<bool> chatInputVisible = new BaseVariable<bool>(false);
        public readonly BaseVariable<bool> avatarNamesVisible = new BaseVariable<bool>(true);
        public readonly BaseVariable<float> avatarNamesOpacity = new BaseVariable<float>(1);
        public readonly BaseVariable<bool> gotoPanelVisible = new BaseVariable<bool>(false);
        public readonly BaseVariable<ParcelCoordinates> gotoPanelCoordinates = new BaseVariable<ParcelCoordinates>(new ParcelCoordinates(0,0));
        public readonly BaseVariable<bool> goToPanelConfirmed = new ();
        public readonly BaseVariable<bool> minimapVisible = new BaseVariable<bool>(true);
        public readonly BaseVariable<bool> jumpHomeButtonVisible = new BaseVariable<bool>(false);
        public readonly BaseVariable<bool> shouldShowNotificationPanel = new BaseVariable<bool>(true);
        public readonly BaseVariable<Transform> notificationPanelTransform = new BaseVariable<Transform>(null);
        public readonly BaseVariable<Transform> topNotificationPanelTransform = new BaseVariable<Transform>(null);
        public readonly BaseVariable<bool> isSceneUIEnabled = new BaseVariable<bool>(true);
        public readonly BaseVariable<HashSet<string>> visibleTaskbarPanels = new BaseVariable<HashSet<string>>(new HashSet<string>());
        public readonly BaseVariable<HashSet<string>> autoJoinChannelList = new BaseVariable<HashSet<string>>(new HashSet<string>());
        public readonly BaseVariable<string> openChat = new BaseVariable<string>("");
        public readonly BaseRefCounter<AvatarModifierAreaID> avatarAreaWarnings = new BaseRefCounter<AvatarModifierAreaID>();
        public readonly BaseVariable<Vector2Int> homePoint = new BaseVariable<Vector2Int>(new Vector2Int(0,0));
        public readonly BaseVariable<Dictionary<int, Queue<IUIRefreshable>>> dirtyShapes = new BaseVariable<Dictionary<int, Queue<IUIRefreshable>>>(new Dictionary<int, Queue<IUIRefreshable>>());
        public readonly BaseVariable<bool> enableFavoritePlaces = new BaseVariable<bool>(false);
        public readonly BaseVariable<int> currentPassportSortingOrder = new BaseVariable<int>();
        public readonly BaseVariable<string> sendFriendRequest = new BaseVariable<string>();
        public readonly BaseVariable<int> sendFriendRequestSource = new BaseVariable<int>();
        public readonly BaseVariable<string> openSentFriendRequestDetail = new BaseVariable<string>();
        public readonly BaseVariable<string> openReceivedFriendRequestDetail = new BaseVariable<string>();
        public readonly BaseVariable<(string playerId, string source)> currentPlayerId = new ((null, null));
    }
}
