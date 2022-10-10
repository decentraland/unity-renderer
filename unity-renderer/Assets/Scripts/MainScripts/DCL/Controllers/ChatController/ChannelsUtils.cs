using System.Collections.Generic;
using UnityEngine;

namespace DCL.Chat
{
    public class ChannelsUtils : IChannelsUtils
    {
        private const string FEATURE_FLAG_FOR_CHANNELS_FEATURE = "matrix_channels_enabled";
        private const string FEATURE_FLAG_FOR_USERS_ALLOWED_TO_CREATE_CHANNELS = "users_allowed_to_create_channels";
        private const string VARIANT_FOR_USERS_ALLOWED_TO_CREATE_CHANNELS = "allowedUsers";

        private BaseVariable<FeatureFlag> featureFlags => DataStore.i.featureFlags.flags;

        private readonly DataStore dataStore;
        private readonly IUserProfileBridge userProfileBridge;

        public ChannelsUtils(DataStore dataStore, IUserProfileBridge userProfileBridge)
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
                    List<string> allowedWallets = new List<string>();
                    if (allowedUsersData.allowList != null)
                    {
                        foreach (var userId in allowedUsersData.allowList)
                            allowedWallets.Add(userId.ToLower());
                    }
                    if (!allowedWallets.Contains(ownUserProfile.userId.ToLower()))
                        return false;
                    break;
                case AllowChannelsCreationMode.NAMES:
                    if (!ownUserProfile.hasClaimedName)
                        return false;
                    break;
                case AllowChannelsCreationMode.WALLET:
                    if (ownUserProfile.isGuest)
                        return false;
                    break;
            }

            return true;
        }
    }
}