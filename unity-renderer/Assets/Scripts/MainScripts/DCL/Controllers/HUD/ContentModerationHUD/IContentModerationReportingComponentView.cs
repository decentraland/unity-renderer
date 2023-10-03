using DCL.Controllers;
using System;
using System.Collections.Generic;

namespace DCL.ContentModeration
{
    public interface IContentModerationReportingComponentView
    {
        event Action OnPanelClosed;
        event Action<(SceneContentCategory contentCategory, List<string> issues, string comments)> OnSendClicked;
        event Action OnLearnMoreClicked;

        void ShowPanel();
        void HidePanel();
        void SetRating(SceneContentCategory contentCategory);
        void SetLoadingState(bool isLoading);
        void SetPanelAsSent(bool hasBeenSent);
        void SetPanelAsError();
    }
}
