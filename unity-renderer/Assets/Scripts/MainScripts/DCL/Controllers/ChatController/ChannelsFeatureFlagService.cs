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

        private BaseVariable<FeatureFlag> featureFlags => dataStore.featureFlags.flags;

        private readonly DataStore dataStore;
        private readonly IUserProfileBridge userProfileBridge;

        public ChannelsFeatureFlagService(DataStore dataStore, IUserProfileBridge userProfileBridge)
        {
            this.dataStore = dataStore;
            this.userProfileBridge = userProfileBridge;
        }

        public bool IsChannelsFeatureEnabled() => featureFlags.Get().IsFeatureEnabled(FEATURE_FLAG_FOR_CHANNELS_FEATURE);

        public bool IsAllowedToCreateChannels()
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