namespace DCL
{
    /// <summary>
    /// Context related to enqueueing and processing of world runtime scene messages.
    /// </summary>
    public class MessagingContext : System.IDisposable
    {
        public readonly MessagingControllersManager manager;

        public MessagingContext(MessagingControllersManager manager)
        {
            this.manager = manager;
        }

        public void Dispose()
        {
            manager.Dispose();
        }
    }
}