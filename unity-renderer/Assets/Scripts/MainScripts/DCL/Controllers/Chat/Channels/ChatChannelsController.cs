using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace DCL.Chat.Channels
{
    public class ChatChannelsController : MonoBehaviour, IChatChannelsBridge
    {
        public event Action OnInitialized;
        public event Action<Channel> OnChannelUpdated;
        public event Action<Channel> OnChannelJoined;
        public event Action<string, string> OnJoinChannelError;
        public event Action<string> OnChannelLeft;
        public event Action<string, string> OnChannelLeaveError;
        public event Action<string, string> OnMuteChannelError;

        private readonly Dictionary<string, Channel> channels = new Dictionary<string, Channel>();

        // called by kernel
        [UsedImplicitly]
        public void InitializeChannels(string payload)
        {
            var msg = JsonUtility.FromJson<InitializeChannelsPayload>(payload);
            // TODO: add unseen notifications
            OnInitialized?.Invoke();
        }

        [UsedImplicitly]
        public void UpdateChannelInfo(string payload)
        {
            var msg = JsonUtility.FromJson<ChannelInfoPayload>(payload);
            var channelId = msg.channelId;
            var channel = new Channel(channelId, msg.unseenMessages, msg.memberCount, msg.joined, msg.muted);
            var justLeft = false;

            if (channels.ContainsKey(channelId))
            {
                justLeft = channels[channelId].Joined && !channel.Joined;
                channels[channelId].CopyFrom(channel);
            }
            else
                channels[channelId] = channel;
            
            if (justLeft)
                OnChannelLeft?.Invoke(channelId);
            
            OnChannelUpdated?.Invoke(channel);
        }

        // called by kernel
        [UsedImplicitly]
        public void JoinChannelConfirmation(string payload)
        {
            var msg = JsonUtility.FromJson<ChannelInfoPayload>(payload);
            var channel = new Channel(msg.channelId, msg.unseenMessages, msg.memberCount, msg.joined, msg.muted);
            OnChannelJoined?.Invoke(channel);
            OnChannelUpdated?.Invoke(channel);
        }
        
        // called by kernel
        [UsedImplicitly]
        public void JoinChannelError(string payload)
        {
            var msg = JsonUtility.FromJson<JoinChannelErrorPayload>(payload);
            OnJoinChannelError?.Invoke(msg.channelId, msg.message);
        }
        
        // called by kernel
        [UsedImplicitly]
        public void LeaveChannelError(string payload)
        {
            var msg = JsonUtility.FromJson<JoinChannelErrorPayload>(payload);
            OnChannelLeaveError?.Invoke(msg.channelId, msg.message);
        }
        
        // called by kernel
        [UsedImplicitly]
        public void MuteChannelError(string payload)
        {
            var msg = JsonUtility.FromJson<MuteChannelErrorPayload>(payload);
            OnMuteChannelError?.Invoke(msg.channelId, msg.message);
        }

        public void JoinOrCreateChannel(string channelId)
        {
            throw new NotImplementedException();
        }

        public void LeaveChannel(string channelId)
        {
            throw new NotImplementedException();
        }

        public void GetMessages(string channelId, int limit, long fromTimestamp)
        {
            throw new NotImplementedException();
        }

        public void GetJoinedChannels(int limit, int skip)
        {
            throw new NotImplementedException();
        }

        public void GetChannels(int limit, int skip, string name)
        {
            throw new NotImplementedException();
        }

        public void MuteChannel(string channelId)
        {
            throw new NotImplementedException();
        }
    }
}