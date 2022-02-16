using System;

namespace DCL
{
    public class DataStore_HUDs
    {
        public readonly BaseVariable<bool> questsPanelVisible = new BaseVariable<bool>(false);
        public readonly BaseVariable<bool> builderProjectsPanelVisible = new BaseVariable<bool>(false);
        public readonly BaseVariable<bool> signupVisible = new BaseVariable<bool>(false);
        public readonly BaseVariable<bool> controlsVisible = new BaseVariable<bool>(false);
        public readonly BaseVariable<bool> isAvatarEditorInitialized = new BaseVariable<bool>(false);
        public readonly BaseVariable<bool> avatarEditorVisible = new BaseVariable<bool>(false);
        public readonly BaseVariable<bool> emotesVisible = new BaseVariable<bool>(false);
        public readonly BaseVariable<bool> isNavMapInitialized = new BaseVariable<bool>(false);
        public readonly BaseVariable<bool> navmapVisible = new BaseVariable<bool>(false);
        public readonly BaseVariable<bool> chatInputVisible = new BaseVariable<bool>(false);
        public readonly BaseVariable<bool> avatarNamesVisible = new BaseVariable<bool>(true);
        public readonly BaseVariable<float> avatarNamesOpacity = new BaseVariable<float>(1);
        public readonly LoadingHUD loadingHUD = new LoadingHUD();

        public class LoadingHUD
        {
            public readonly BaseVariable<bool> visible = new BaseVariable<bool>(false);
            public readonly BaseVariable<string> message = new BaseVariable<string>(null);
            public readonly BaseVariable<float> percentage = new BaseVariable<float>(0);
            public readonly BaseVariable<bool> showTips = new BaseVariable<bool>(false);
            public readonly BaseVariable<Exception> fatalError = new BaseVariable<Exception>();
        }
    }
}