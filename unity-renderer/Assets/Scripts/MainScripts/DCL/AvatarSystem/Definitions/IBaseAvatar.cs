using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AvatarSystem
{
    public interface IBaseAvatar
    {
        void Initialize();
        void FadeIn();
        void FadeOut();
    }
}
