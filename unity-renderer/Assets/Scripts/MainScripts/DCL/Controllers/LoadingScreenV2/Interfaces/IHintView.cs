using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.LoadingScreen.V2
{
    public interface IHintView
    {
        void Initialize(Hint hint, Texture2D texture, float fadeDuration, bool startAsActive);
        void ToggleHint(bool active);
    }
}

