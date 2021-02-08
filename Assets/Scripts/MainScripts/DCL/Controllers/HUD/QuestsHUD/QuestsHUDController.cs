namespace DCL.Huds
{
    public class QuestsHUDController : IHUD
    {
        private QuestsHUDView view;

        public void Dispose()
        {
            throw new System.NotImplementedException();
        }

        public void Initialize()
        {
            view = QuestsHUDView.Create();
        }

        public void SetVisibility(bool visible)
        {
            view?.gameObject.SetActive(visible);
        }
    }
}