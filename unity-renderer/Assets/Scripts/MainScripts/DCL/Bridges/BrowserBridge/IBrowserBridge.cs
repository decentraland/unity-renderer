namespace DCL.Browser
{
    public interface IBrowserBridge
    {
        void OpenUrl(string url);
        void LogOut();
    }
}
