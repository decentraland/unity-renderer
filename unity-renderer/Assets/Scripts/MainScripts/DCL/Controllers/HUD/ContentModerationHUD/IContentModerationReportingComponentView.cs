using DCL.Controllers;
using System;

namespace DCL.ContentModeration
{
    public interface IContentModerationReportingComponentView
    {
        event Action OnPanelClosed;

        void ShowPanel();
        void HidePanel();
        void SetRating(SceneContentCategory contentCategory);
    }
}
