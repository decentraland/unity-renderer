namespace DCL
{
    /// <summary>
    /// Context related to enqueueing and processing of world runtime scene messages.
    /// </summary>
    public class MessagingContext : System.IDisposable
    {
        public readonly IMessagingControllersManager manager;

        public MessagingContext(IMessagingControllersManager manager)
        {
            this.manager = manager;
        }

        public void Dispose()
        {
            manager.Dispose();
        }
    }
}