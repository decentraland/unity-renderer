using NSubstitute;

namespace DCL.Tests
{
    public static class MessagingContextFactory
    {
        public static MessagingContext CreateMocked()
        {
            return CreateWithCustomMocks();
        }

        public static MessagingContext CreateWithCustomMocks(IMessagingControllersManager messagingControllersManager = null)
        {
            return new MessagingContext(messagingControllersManager ?? Substitute.For<IMessagingControllersManager>());
        }
    }
}