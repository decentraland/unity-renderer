using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DCL;
using AvatarSystem;

public interface IBaseAvatarRevealer
{
    void InjectLodSystem(ILOD lod);
    void AddTarget(MeshRenderer newTarget);
    void StartAvatarRevealAnimation(bool closeby);
    SkinnedMeshRenderer GetMainRenderer();
    void Reset();
}
