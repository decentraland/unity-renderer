using DCL;
using DCL.Chat;
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
        UserProfileWebInterfaceBridge userProfileWebInterfaceBridge = new UserProfileWebInterfaceBridge();

        joinChannelComponentController = new JoinChannelComponentController(
            JoinChannelComponentView.Create(),
            ChatController.i,
            DataStore.i,
            new SocialAnalytics(
                Environment.i.platform.serviceProviders.analytics,
                userProfileWebInterfaceBridge),
            Resources.Load<StringVariable>("CurrentPlayerInfoCardId"),
            new ChannelsUtils(
                DataStore.i,
                userProfileWebInterfaceBridge));
    }

    public void Dispose() { joinChannelComponentController.Dispose(); }
}
