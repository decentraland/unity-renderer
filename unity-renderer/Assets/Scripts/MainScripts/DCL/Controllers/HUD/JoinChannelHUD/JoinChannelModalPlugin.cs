using DCL;
using DCL.Chat;
using DCL.Chat.Channels;
using DCL.Social.Chat;
using DCL.Social.Chat.Channels;
using SocialFeaturesAnalytics;
using UnityEngine;

/// <summary>
/// Plugin feature that initialize the Join Channel Modal feature.
/// </summary>
public class JoinChannelModalPlugin : IPlugin
{
    private readonly JoinChannelComponentController joinChannelComponentController;

    public JoinChannelModalPlugin()
    {
        ServiceLocator serviceLocator = Environment.i.serviceLocator;

        joinChannelComponentController = new JoinChannelComponentController(
            JoinChannelComponentView.Create(),
            serviceLocator.Get<IChatController>(),
            DataStore.i,
            new SocialAnalytics(
                Environment.i.platform.serviceProviders.analytics,
                new UserProfileWebInterfaceBridge()),
            serviceLocator.Get<IChannelsFeatureFlagService>());
    }

    public void Dispose() { joinChannelComponentController.Dispose(); }
}
