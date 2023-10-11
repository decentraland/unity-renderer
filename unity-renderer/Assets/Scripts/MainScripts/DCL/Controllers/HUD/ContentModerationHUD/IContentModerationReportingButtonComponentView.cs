using DCL.Controllers;
using System;

namespace DCL.ContentModeration
{
    public interface IContentModerationReportingButtonComponentView
    {
        event Action OnContentModerationPressed;

        void Show();
        void Hide();
        void SetContentCategory(SceneContentCategory contentCategory);
    }
}
