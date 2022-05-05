using NSubstitute;

namespace SocialFeaturesAnalytics.TestHelpers
{
    public class SocialAnalyticsTestHelpers
    {
        public static ISocialAnalytics CreateMockedSocialAnalytics()
        {
            ISocialAnalytics mockedSocialAnalytics = Substitute.For<ISocialAnalytics>();

            mockedSocialAnalytics.When(x => x.SendPlayerMuted(Arg.Any<string>())).Do(x => { });
            mockedSocialAnalytics.When(x => x.SendPlayerUnmuted(Arg.Any<string>())).Do(x => { });
            mockedSocialAnalytics.When(x => x.SendVoiceMessageSent(Arg.Any<double>())).Do(x => { });
            mockedSocialAnalytics.When(x => x.SendChannelMessageSent(Arg.Any<string>(), Arg.Any<double>(), Arg.Any<string>(), Arg.Any<ChatMessageType>())).Do(x => { });
            mockedSocialAnalytics.When(x => x.SendChannelMessageReceived(Arg.Any<string>(), Arg.Any<double>(), Arg.Any<string>(), Arg.Any<ChatMessageType>())).Do(x => { });
            mockedSocialAnalytics.When(x => x.SendDirectMessageSent(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<double>(), Arg.Any<bool>(), Arg.Any<ChatContentType>())).Do(x => { });
            mockedSocialAnalytics.When(x => x.SendDirectMessageReceived(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<double>(), Arg.Any<bool>(), Arg.Any<ChatContentType>())).Do(x => { });
            mockedSocialAnalytics.When(x => x.SendDirectMessageReceived(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<double>(), Arg.Any<bool>(), Arg.Any<ChatContentType>())).Do(x => { });
            mockedSocialAnalytics.When(x => x.SendFriendRequestSent(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<double>(), Arg.Any<FriendActionSource>())).Do(x => { });
            mockedSocialAnalytics.When(x => x.SendFriendRequestApproved(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<FriendActionSource>())).Do(x => { });
            mockedSocialAnalytics.When(x => x.SendFriendRequestRejected(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<FriendActionSource>())).Do(x => { });
            mockedSocialAnalytics.When(x => x.SendFriendRequestCancelled(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<FriendActionSource>())).Do(x => { });
            mockedSocialAnalytics.When(x => x.SendFriendDeleted(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<FriendActionSource>())).Do(x => { });
            mockedSocialAnalytics.When(x => x.SendPassportOpen()).Do(x => { });
            mockedSocialAnalytics.When(x => x.SendPassportClose(Arg.Any<double>())).Do(x => { });
            mockedSocialAnalytics.When(x => x.SendPlayerBlocked(Arg.Any<bool>(), Arg.Any<FriendActionSource>())).Do(x => { });
            mockedSocialAnalytics.When(x => x.SendPlayerUnblocked(Arg.Any<bool>(), Arg.Any<FriendActionSource>())).Do(x => { });
            mockedSocialAnalytics.When(x => x.SendPlayerReport(Arg.Any<PlayerReportIssueType>(), Arg.Any<double>(), Arg.Any<FriendActionSource>())).Do(x => { });
            mockedSocialAnalytics.When(x => x.SendPlayerJoin(Arg.Any<FriendActionSource>())).Do(x => { });
            mockedSocialAnalytics.When(x => x.SendPlayEmote(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<EmoteSource>())).Do(x => { });

            return mockedSocialAnalytics;
        }
    }
}