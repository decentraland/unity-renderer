using System;
using UIComponents.Scripts.Components;

namespace DCL.Social.Chat
{
    public interface IChatInputContextMenuView : IBaseComponentView<ChatInputContextualMenuModel>
    {
        event Action<string> OnSelectionChanged;
        event Action OnShowRequested;
        event Action OnPasteRequested;
        event Action<string> OnCopyRequested;

        void Paste(string text);
    }
}
