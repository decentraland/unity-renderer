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

        void StartCarousel(CancellationToken ct);
        void StopCarousel();
        void CarouselNextHint(CancellationToken ct);
        void CarouselPreviousHint(CancellationToken ct);
        void SetSpecificHint(int index, CancellationToken ct);
    }
}

