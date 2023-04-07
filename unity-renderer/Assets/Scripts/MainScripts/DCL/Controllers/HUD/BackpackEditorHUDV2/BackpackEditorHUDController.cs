using System;

namespace DCL.Backpack
{
    public class BackpackEditorHUDController : IHUD
    {
        private readonly IBackpackEditorHUDView view;

        public BackpackEditorHUDController(IBackpackEditorHUDView view)
        {
            this.view = view;
        }

        public void Dispose()
        {
            view.Dispose();
        }

        public void SetVisibility(bool visible)
        {
            throw new NotImplementedException();
        }
    }
}
