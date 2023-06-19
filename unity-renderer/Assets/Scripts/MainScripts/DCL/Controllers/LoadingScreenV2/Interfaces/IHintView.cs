using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.LoadingScreen.V2
{
    public interface IHintView
    {
        void Initialize(Hint hint, Texture2D texture, bool startAsActive = false);
        void ToggleHint(bool active);
    }
}

