using System;

namespace DCL.Chat.Channels
{
    public interface IChatChannelsBridge
    {
        event Action OnInitialized;
        event Action<Channel> OnChannelUpdated;
        event Action<Channel> OnChannelJoined;
        event Action<string, string> OnJoinChannelError;
        event Action<string> OnChannelLeft;
        event Action<string, string> OnChannelLeaveError;
        event Action<string, string> OnMuteChannelError; 

        void JoinOrCreateChannel(string channelId);
        void LeaveChannel(string channelId);
        void GetMessages(string channelId, int limit, long fromTimestamp);
        void GetJoinedChannels(int limit, int skip);
        void GetChannels(int limit, int skip, string name);
        void MuteChannel(string channelId);
    }
}