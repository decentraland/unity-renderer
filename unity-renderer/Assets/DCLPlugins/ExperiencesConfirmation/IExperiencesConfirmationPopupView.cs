using System;
using UIComponents.Scripts.Components;

namespace DCL.PortableExperiences.Confirmation
{
    public interface IExperiencesConfirmationPopupView : IBaseComponentView<ExperiencesConfirmationViewModel>
    {
        event Action OnAccepted;
        event Action OnRejected;
        event Action OnCancelled;
        event Action OnDontShowAnymore;
        event Action OnKeepShowing;
    }
}
