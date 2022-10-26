using DCL.Components.Interfaces;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace DCL
{
    public class DataStore_HUDs
    {
        public readonly BaseVariable<bool> connectWalletModalVisible = new BaseVariable<bool>(false);
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
        public readonly BaseVariable<bool> chatInputVisible = new BaseVariable<bool>(false);
        public readonly BaseVariable<bool> avatarNamesVisible = new BaseVariable<bool>(true);
        public readonly BaseVariable<float> avatarNamesOpacity = new BaseVariable<float>(1);
        public readonly BaseVariable<bool> gotoPanelVisible = new BaseVariable<bool>(false);
        public readonly BaseVariable<ParcelCoordinates> gotoPanelCoordinates = new BaseVariable<ParcelCoordinates>(new ParcelCoordinates(0,0));
        public readonly BaseVariable<bool> shouldShowNotificationPanel = new BaseVariable<bool>(true);
        public readonly BaseVariable<Transform> notificationPanelTransform = new BaseVariable<Transform>(null);
        public readonly BaseVariable<Transform> topNotificationPanelTransform = new BaseVariable<Transform>(null);
        public readonly BaseVariable<bool> isSceneUIEnabled = new BaseVariable<bool>(true);
        public readonly BaseVariable<HashSet<string>> visibleTaskbarPanels = new BaseVariable<HashSet<string>>(new HashSet<string>());
        public readonly BaseVariable<string> openedChat = new BaseVariable<string>("");
        public readonly BaseRefCounter<AvatarModifierAreaID> avatarAreaWarnings = new BaseRefCounter<AvatarModifierAreaID>();
        public readonly BaseVariable<Vector2Int> homePoint = new BaseVariable<Vector2Int>(new Vector2Int(0,0));
        public readonly BaseVariable<Dictionary<string, Queue<IUIRefreshable>>> dirtyShapes = new BaseVariable<Dictionary<string, Queue<IUIRefreshable>>>(new Dictionary<string, Queue<IUIRefreshable>>());
        public readonly LoadingHUD loadingHUD = new LoadingHUD();

        public class LoadingHUD
        {
            public readonly BaseVariable<bool> visible = new BaseVariable<bool>(false);
            public readonly BaseVariable<bool> fadeIn = new BaseVariable<bool>(false);
            public readonly BaseVariable<bool> fadeOut = new BaseVariable<bool>(false);
            public readonly BaseVariable<string> message = new BaseVariable<string>(null);
            public readonly BaseVariable<float> percentage = new BaseVariable<float>(0);
            public readonly BaseVariable<bool> showTips = new BaseVariable<bool>(false);
            public readonly BaseVariable<Exception> fatalError = new BaseVariable<Exception>();
        }
    }
}