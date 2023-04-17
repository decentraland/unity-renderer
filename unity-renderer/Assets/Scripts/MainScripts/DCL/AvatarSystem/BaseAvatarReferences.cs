using System;
using UnityEngine;

namespace AvatarSystem
{
    [Serializable]
    public class BaseAvatarReferences : MonoBehaviour, IBaseAvatarReferences
    {
        [Header("References")]
        public SkinnedMeshRenderer skinnedMeshRenderer;
        public Transform armatureContainer;
        public GameObject particlesContainer;

        [Header("Ghost Settings")]
        [ColorUsage(true, true)] public Color ghostMinColor;
        [ColorUsage(true, true)] public Color ghostMaxColor;
        public float fadeGhostSpeed = 2f;
        public float revealSpeed = 2f;

        public SkinnedMeshRenderer SkinnedMeshRenderer => skinnedMeshRenderer;
        public Transform ArmatureContainer => armatureContainer;
        public GameObject ParticlesContainer => particlesContainer;
        public Color GhostMinColor => ghostMinColor;
        public Color GhostMaxColor => ghostMaxColor;
        public float FadeGhostSpeed => fadeGhostSpeed;
        public float RevealSpeed => revealSpeed;

        public void Dispose()
        {
            Destroy(gameObject);
        }
    }
}
