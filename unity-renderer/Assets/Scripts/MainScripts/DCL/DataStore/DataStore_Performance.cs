namespace DCL
{
    public class DataStore_Performance
    {
        public readonly BaseVariable<bool> multithreading = new BaseVariable<bool>(false);
        public readonly BaseVariable<int> maxDownloads = new BaseVariable<int>(10);
        public readonly BaseVariable<bool> throttleMessageProcessing = new BaseVariable<bool>(true);
        public readonly BaseVariable<bool> throttleGLTF = new BaseVariable<bool>(true);
    }
}