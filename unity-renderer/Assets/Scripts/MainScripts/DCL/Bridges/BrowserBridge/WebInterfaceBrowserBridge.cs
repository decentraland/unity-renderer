using DCL.Interface;

namespace DCL.Browser
{
    public class WebInterfaceBrowserBridge : IBrowserBridge
    {
        public void OpenUrl(string url)
        {
            WebInterface.OpenURL(url);
        }
    }
}
