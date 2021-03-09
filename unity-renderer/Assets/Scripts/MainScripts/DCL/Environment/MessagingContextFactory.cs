namespace DCL
{
    public static class MessagingContextFactory
    {
        public static MessagingContext CreateDefault()
        {
            return new MessagingContext(new MessagingControllersManager());
        }
    }
}