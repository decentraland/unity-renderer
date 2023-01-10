using System;

namespace DCL.LoadingScreen
{
    public interface ILoadingScreenView : IBaseComponentView
    {
        void FadeOut();

        void FadeIn(bool instant, bool blitTexture);

        event Action<ShowHideAnimator> OnFadeInFinish;

        LoadingScreenTipsView GetTipsView();

        LoadingScreenPercentageView GetPercentageView();
    }
}
