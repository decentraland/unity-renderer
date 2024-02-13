using DCL.LoadingScreen.V2;
using System;
using UnityEngine;

namespace DCL.LoadingScreen
{
    public interface ILoadingScreenView : IBaseComponentView
    {
        void FadeOut();

        void FadeIn(bool instant, bool blitTexture);

        event Action<ShowHideAnimator> OnFadeInFinish;

        LoadingScreenPercentageView GetPercentageView();

        ILoadingScreenTimeoutView GetTimeoutView();

        RectTransform GetHintContainer();

        LoadingScreenV2HintsPanelView GetHintsPanelView();

        void ToggleLoadingScreenV2(bool active);
    }
}
