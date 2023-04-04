using System;
using UnityEngine;

namespace AvatarSystem
{
    public interface IBaseAvatarReferences : IDisposable
    {
        SkinnedMeshRenderer SkinnedMeshRenderer { get; }
        Transform ArmatureContainer { get; }
        GameObject ParticlesContainer { get; }

        Color GhostMinColor { get; }
        Color GhostMaxColor { get; }

        float FadeGhostSpeed { get; }
        float RevealSpeed { get; }
    }
}
