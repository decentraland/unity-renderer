using System;

namespace DCL.Social.Chat
{
    public interface IChannelLinkDetectorView
    {
        event Action<string> OnClicked;

        void Enable();
    }
}
