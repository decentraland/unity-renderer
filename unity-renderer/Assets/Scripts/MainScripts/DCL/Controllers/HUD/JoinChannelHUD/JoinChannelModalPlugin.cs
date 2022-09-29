using DCL;
using DCL.Chat.Channels;
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
        joinChannelComponentController = new JoinChannelComponentController(
            JoinChannelComponentView.Create(),
            ChatController.i,
            DataStore.i,
            new SocialAnalytics(
                Environment.i.platform.serviceProviders.analytics,
                new UserProfileWebInterfaceBridge()),
            Resources.Load<StringVariable>("CurrentPlayerInfoCardId"));
    }

    public void Dispose() { joinChannelComponentController.Dispose(); }
}
