using System;

namespace DCL.Social.Chat.Channels
{
    public interface IJoinChannelComponentView
    {
        event Action OnCancelJoin;
        event Action<string> OnConfirmJoin;

        void SetChannel(string channelId);
        void Show(bool instant = false);
        void Hide(bool instant = false);
    }
}
