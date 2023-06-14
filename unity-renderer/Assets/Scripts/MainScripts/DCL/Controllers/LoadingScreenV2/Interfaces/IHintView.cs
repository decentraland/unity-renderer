using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Controllers.LoadingScreenV2
{
    public interface IHintView
    {
        void Initialize(Hint hint, Texture2D texture, bool startAsActive = false);
        void ToggleHint(bool active);
    }
}

