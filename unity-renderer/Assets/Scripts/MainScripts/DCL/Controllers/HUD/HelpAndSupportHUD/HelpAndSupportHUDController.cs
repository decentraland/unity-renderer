namespace DCL.HelpAndSupportHUD
{
    public class HelpAndSupportHUDController : IHUD
    {
        public HelpAndSupportHUDView view { private set; get; }

        public HelpAndSupportHUDController()
        {
            view = HelpAndSupportHUDView.Create();
        }

        public void SetVisibility(bool visible)
        {
            view.SetVisibility(visible);
        }

        public void Dispose()
        {
            if (view != null)
                UnityEngine.Object.Destroy(view.gameObject);
        }
    }
}