using DCL.Controllers.LoadingScreenV2;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace DCL.Controllers.LoadingScreenV2
{
    public interface IHintViewManager
    {
        event Action OnHintChanged;

        void StartCarousel();
        void StopCarousel();
        void CarouselNextHint();
        void CarouselPreviousHint();
        void SetSpecificHint(int index);
    }
}

