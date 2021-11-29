namespace DCL
{
    /// <summary>
    /// Context related to enqueueing and processing of world runtime scene messages.
    /// </summary>
    public class MessagingContext
    {
        private ServiceLocator serviceLocator;
        public IMessagingControllersManager manager => serviceLocator.Get<IMessagingControllersManager>();

        public MessagingContext (ServiceLocator serviceLocator)
        {
            this.serviceLocator = serviceLocator;
        }
    }
}