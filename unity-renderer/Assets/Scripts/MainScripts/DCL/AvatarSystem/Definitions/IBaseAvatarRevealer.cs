using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using DCL;
using AvatarSystem;
using Cysharp.Threading.Tasks;

public interface IBaseAvatarRevealer
{
    void InjectLodSystem(ILOD lod);
    void AddTarget(MeshRenderer newTarget);
    UniTask StartAvatarRevealAnimation(bool instant, CancellationToken token);
    SkinnedMeshRenderer GetMainRenderer();
    void Reset();
}
