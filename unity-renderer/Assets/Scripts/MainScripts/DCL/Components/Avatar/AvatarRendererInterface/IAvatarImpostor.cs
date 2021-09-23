using System;
using UnityEngine;

namespace DCL
{
    public interface IAvatarImpostor : IDisposable
    {
        public event Action<float> OnImpostorAlphaValueUpdate;
        void PopulateTexture(string userId);
        void CleanUp();
        void SetFade(float impostorFade);
        void SetVisibility(bool impostorVisibility);
        void SetForward(Vector3 newForward);
        void SetColor(Color newColor);
        void SetColorByDistance(float distance);
    }
}