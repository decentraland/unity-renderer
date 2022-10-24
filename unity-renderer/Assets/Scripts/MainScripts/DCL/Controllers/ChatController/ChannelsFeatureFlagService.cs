using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DCL.Chat.Channels
{
    public class ChannelsFeatureFlagService : IChannelsFeatureFlagService
    {
        private const string FEATURE_FLAG_FOR_CHANNELS_FEATURE = "matrix_channels_enabled";
        private const string FEATURE_FLAG_FOR_USERS_ALLOWED_TO_CREATE_CHANNELS = "users_allowed_to_create_channels";
        private const string VARIANT_FOR_USERS_ALLOWED_TO_CREATE_CHANNELS = "allowedUsers";
        private const string FEATURE_FLAG_FOR_AUTOMATIC_JOIN_CHANNELS = "automatic_joined_channels";
        private const string VARIANT_FOR_AUTOMATIC_JOIN_CHANNELS = "automaticChannels";

        private BaseVariable<FeatureFlag> featureFlags => dataStore.featureFlags.flags;

        private readonly DataStore dataStore;
        private readonly IUserProfileBridge userProfileBridge;

        public event Action<bool> OnAllowedToCreateChannelsChanged;
        public event Action<AutomaticJoinChannelList> OnAutoJoinChannelsChanged;

        public ChannelsFeatureFlagService(DataStore dataStore, IUserProfileBridge userProfileBridge)
        {
            this.dataStore = dataStore;
            this.userProfileBridge = userProfileBridge;

            this.userProfileBridge.GetOwn().OnUpdate += OnUserProfileUpdate;
        }

        public bool IsChannelsFeatureEnabled() => featureFlags.Get().IsFeatureEnabled(FEATURE_FLAG_FOR_CHANNELS_FEATURE);

        private void OnUserProfileUpdate(UserProfile profile)
        {
            if (string.IsNullOrEmpty(profile.userId))
                return;

            OnAllowedToCreateChannelsChanged?.Invoke(IsAllowedToCreateChannels());
            OnAutoJoinChannelsChanged?.Invoke(GetAutoJoinChannelsList());
        }

        private AutomaticJoinChannelList GetAutoJoinChannelsList()
        {
            if (!featureFlags.Get().IsFeatureEnabled(FEATURE_FLAG_FOR_AUTOMATIC_JOIN_CHANNELS))
                return null;

            FeatureFlagVariantPayload ffChannelsList = featureFlags
                .Get()
                .GetFeatureFlagVariantPayload($"{FEATURE_FLAG_FOR_AUTOMATIC_JOIN_CHANNELS}:{VARIANT_FOR_AUTOMATIC_JOIN_CHANNELS}");

            AutomaticJoinChannelList autoJoinChannelsList = JsonUtility.FromJson<AutomaticJoinChannelList>(ffChannelsList.value);
            return autoJoinChannelsList;
        }

        private bool IsAllowedToCreateChannels()
        {
            if (!featureFlags.Get().IsFeatureEnabled(FEATURE_FLAG_FOR_USERS_ALLOWED_TO_CREATE_CHANNELS))
                return false;

            FeatureFlagVariantPayload usersAllowedToCreateChannelsPayload = featureFlags
                .Get()
                .GetFeatureFlagVariantPayload($"{FEATURE_FLAG_FOR_USERS_ALLOWED_TO_CREATE_CHANNELS}:{VARIANT_FOR_USERS_ALLOWED_TO_CREATE_CHANNELS}");

            if (usersAllowedToCreateChannelsPayload == null)
                return false;

            UsersAllowedToCreateChannelsVariantPayload allowedUsersData = JsonUtility.FromJson<UsersAllowedToCreateChannelsVariantPayload>(usersAllowedToCreateChannelsPayload.value);
            UserProfile ownUserProfile = userProfileBridge.GetOwn();

            switch (allowedUsersData.mode)
            {
                case AllowChannelsCreationMode.ALLOWLIST:
                    return allowedUsersData.allowList.Any(userId => userId.ToLower() == ownUserProfile.userId.ToLower());
                case AllowChannelsCreationMode.NAMES:
                    return ownUserProfile.hasClaimedName;
                case AllowChannelsCreationMode.WALLET:
                    return !ownUserProfile.isGuest;
            }

            return true;
        }
    }
}