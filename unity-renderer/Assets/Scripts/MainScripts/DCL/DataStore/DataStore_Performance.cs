namespace DCL
{
    public class DataStore_Performance
    {
        public readonly BaseVariable<bool> multithreading = new BaseVariable<bool>(false);
        public readonly BaseVariable<int> maxDownloads = new BaseVariable<int>(100);
    }
}