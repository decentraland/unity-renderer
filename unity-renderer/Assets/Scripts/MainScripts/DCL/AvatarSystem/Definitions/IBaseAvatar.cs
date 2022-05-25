using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AvatarSystem
{
    public interface IBaseAvatar
    {
        void Initialize(bool resetLoading);
        void FadeIn();
        void FadeOut(Renderer targetRenderer);
    }
}
