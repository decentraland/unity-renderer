using System;

namespace DCL
{
    public interface INavMapLocationControlsView
    {
        event Action HomeButtonClicked;
        event Action CenterToPlayerButtonClicked;

        void Hide();
    }
}
