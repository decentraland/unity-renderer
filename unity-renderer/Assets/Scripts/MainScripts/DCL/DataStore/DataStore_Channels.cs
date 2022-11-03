namespace DCL
{
    public class DataStore_Channels
    {
        public readonly BaseVariable<string> currentJoinChannelModal = new BaseVariable<string>(null);
        public readonly BaseVariable<ChannelJoinedSource> channelJoinedSource = new BaseVariable<ChannelJoinedSource>(ChannelJoinedSource.Unknown);
        public readonly BaseVariable<ChannelLeaveSource> channelLeaveSource = new BaseVariable<ChannelLeaveSource>(ChannelLeaveSource.Unknown);
        public readonly BaseVariable<bool> isCreationModalVisible = new BaseVariable<bool>();
        public readonly BaseVariable<string> currentChannelLimitReached = new BaseVariable<string>();
        public readonly BaseVariable<string> joinChannelError = new BaseVariable<string>();
        public readonly BaseVariable<string> leaveChannelError = new BaseVariable<string>();
        public readonly BaseVariable<string> channelToBeOpenedFromLink = new BaseVariable<string>();
        public readonly BaseVariable<bool> isPromoteToastVisible = new BaseVariable<bool>();
    }
}
