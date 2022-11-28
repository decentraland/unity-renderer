namespace DCL
{
    public interface IRPC : IService
    {
        public ClientEmotesKernelService emotes { get; internal set; }

        public ClientAnalyticsKernelService analytics { get; internal set; }
    }
}
