using DCL.Components.Interfaces;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace DCL
{
    public class DataStore_HUDs
    {
        public readonly BaseVariable<bool> connectWalletModalVisible = new (false);
        public readonly BaseVariable<bool> closedWalletModal = new (false);
        public readonly BaseVariable<bool> questsPanelVisible = new (false);
        public readonly BaseVariable<bool> builderProjectsPanelVisible = new (false);
        public readonly BaseVariable<bool> signupVisible = new (false);
        public readonly BaseVariable<bool> controlsVisible = new (false);
        public readonly BaseVariable<bool> isCameraReelInitialized = new (false);
        public readonly BaseVariable<bool> cameraReelSectionVisible = new (false);
        public readonly BaseVariable<string> cameraReelOpenSource = new ();
        public readonly BaseVariable<bool> isAvatarEditorInitialized = new (false);
        public readonly BaseVariable<bool> avatarEditorVisible = new (false);
        public readonly BaseVariable<bool> emotesVisible = new (false);
        public readonly BaseVariable<bool> emoteJustTriggeredFromShortcut = new (false);
        public readonly BaseVariable<bool> isNavMapInitialized = new (false);
        public readonly BaseVariable<bool> navmapVisible = new (false);
        public readonly BaseVariable<bool> navmapIsRendered = new (false);
        public readonly BaseVariable<Texture> latestDownloadedMainTexture = new (null);
        public readonly BaseVariable<Texture> latestDownloadedMapEstatesTexture = new (null);
        public readonly BaseVariable<Texture> mapMainTexture = new (null);
        public readonly BaseVariable<Texture> mapEstatesTexture = new (null);
        public readonly BaseVariable<bool> chatInputVisible = new (false);
        public readonly BaseVariable<bool> avatarNamesVisible = new (true);
        public readonly BaseVariable<float> avatarNamesOpacity = new (1);
        public readonly BaseVariable<bool> gotoPanelVisible = new (false);
        public readonly BaseVariable<(ParcelCoordinates coordinates, string realm, Action onAcceptedCallback)> gotoPanelCoordinates = new ();
        public readonly BaseVariable<bool> goToPanelConfirmed = new ();
        public readonly BaseVariable<bool> minimapVisible = new (true);
        public readonly BaseVariable<bool> jumpHomeButtonVisible = new (false);
        public readonly BaseVariable<bool> shouldShowNotificationPanel = new (true);
        public readonly BaseVariable<Transform> notificationPanelTransform = new (null);
        public readonly BaseVariable<Transform> topNotificationPanelTransform = new (null);
        public readonly BaseVariable<bool> isCurrentSceneUiEnabled = new (true);
        public readonly BaseDictionary<int, bool> isSceneUiEnabled = new ();
        public readonly BaseVariable<HashSet<string>> visibleTaskbarPanels = new (new HashSet<string>());
        public readonly BaseVariable<HashSet<string>> autoJoinChannelList = new (new HashSet<string>());
        public readonly BaseVariable<string> openChat = new ("");
        public readonly BaseRefCounter<AvatarModifierAreaID> avatarAreaWarnings = new ();
        public readonly BaseVariable<Vector2Int> homePoint = new (new Vector2Int(0, 0));
        public readonly BaseVariable<Dictionary<int, Queue<IUIRefreshable>>> dirtyShapes = new (new Dictionary<int, Queue<IUIRefreshable>>());
        public readonly BaseVariable<bool> enableFavoritePlaces = new (false);
        public readonly BaseVariable<int> currentPassportSortingOrder = new ();
        public readonly BaseVariable<string> sendFriendRequest = new ();
        public readonly BaseVariable<int> sendFriendRequestSource = new ();
        public readonly BaseVariable<string> openSentFriendRequestDetail = new ();
        public readonly BaseVariable<string> openReceivedFriendRequestDetail = new ();
        public readonly BaseVariable<(string playerId, string source)> currentPlayerId = new ((null, null));
        public readonly BaseVariable<bool> tosPopupVisible = new (false);
        public readonly BaseVariable<bool> enableOutfits = new (false);
    }
}
