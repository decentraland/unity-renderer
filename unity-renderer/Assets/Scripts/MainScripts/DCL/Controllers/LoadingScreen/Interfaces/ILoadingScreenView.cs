using System;

namespace DCL.LoadingScreen
{
    public interface ILoadingScreenView : IBaseComponentView
    {
        void UpdateLoadingMessage();

        void FadeOut();

        void FadeIn(bool instant);

        event Action<ShowHideAnimator> OnFadeInFinish;
    }
}
