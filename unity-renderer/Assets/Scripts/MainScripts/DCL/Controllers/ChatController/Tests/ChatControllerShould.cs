using DCL.Social.Chat;
using NSubstitute;
using NUnit.Framework;

namespace DCL.Chat.Channels
{
    public class ChatControllerShould
    {
        private ChatController controller;

        [SetUp]
        public void SetUp()
        {
            controller = new ChatController(Substitute.For<IChatApiBridge>(), new DataStore());
        }
    }
}