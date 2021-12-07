using DCL;
using UnityEngine;

namespace AvatarSystem
{
    public interface IAnimator
    {
        bool Prepare( string bodyshapeId, GameObject container);
    }
}