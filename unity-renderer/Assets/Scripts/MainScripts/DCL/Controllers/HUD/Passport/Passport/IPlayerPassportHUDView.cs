using System;
using UnityEngine;

namespace DCL.Social.Passports
{
    public interface IPlayerPassportHUDView
    {
        event Action OnClose;

        void Initialize();
        void SetVisibility(bool visible);
        void SetPassportPanelVisibility(bool visible);
        void Dispose();
    }
}