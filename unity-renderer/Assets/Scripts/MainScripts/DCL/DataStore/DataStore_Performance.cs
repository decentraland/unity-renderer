namespace DCL
{
    public class DataStore_Performance
    {
        public readonly BaseVariable<bool> multithreading = new (false);
        public readonly BaseVariable<int> maxDownloads = new (10);
        public readonly BaseVariable<int> concurrentABRequests = new ();
    }
}
