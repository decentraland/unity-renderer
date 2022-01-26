namespace DCL
{
    /// <summary>
    /// Context related to enqueueing and processing of world runtime scene messages.
    /// </summary>
    [System.Obsolete("This is kept for retrocompatibilty and will be removed in the future. Use Environment.i.serviceLocator instead.")]
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