using System;
using UnityEngine;

namespace AvatarSystem
{
    public interface IBaseAvatarReferences : IDisposable
    {
        SkinnedMeshRenderer SkinnedMeshRenderer { get; }
        Transform ArmatureContainer { get; }
        public GameObject ParticlesContainer { get; }

        Color GhostMinColor { get; }
        Color GhostMaxColor { get; }

        float FadeGhostSpeed { get; }
        float RevealSpeed { get; }
    }
}
