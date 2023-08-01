using System.Collections.Generic;

namespace DCL.Social.Chat
{
    public class ChatInputContextualMenuPlugin : IPlugin
    {
        private readonly List<ChatInputContextualMenuController> controllers = new ();

        public ChatInputContextualMenuPlugin()
        {
            ChatInputContextualMenuView.instances.OnAdded += view =>
            {
                controllers.Add(new ChatInputContextualMenuController(view, Clipboard.Create()));
            };
        }

        public void Dispose()
        {
            foreach (ChatInputContextualMenuController controller in controllers)
                controller.Dispose();

            controllers.Clear();
        }
    }
}
