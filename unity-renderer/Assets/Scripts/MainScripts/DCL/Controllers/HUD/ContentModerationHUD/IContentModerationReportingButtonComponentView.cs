using DCL.Controllers;
using System;

namespace DCL.ContentModeration
{
    public interface IContentModerationReportingButtonComponentView
    {
        event Action OnContentModerationPressed;

        void SetContentCategory(SceneContentCategory contentCategory);
    }
}
