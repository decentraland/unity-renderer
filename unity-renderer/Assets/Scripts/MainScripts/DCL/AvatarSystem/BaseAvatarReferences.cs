using System;
using UnityEngine;

namespace AvatarSystem
{
    [Serializable]
    public struct AvatarAnchorPoint
    {
        public string AnchorName;
        public Transform Bone;
    }

    [Serializable]
    public class BaseAvatarReferences : MonoBehaviour, IBaseAvatarReferences
    {
        [Header("References")]
        public SkinnedMeshRenderer skinnedMeshRenderer;
        public Transform armatureContainer;
        public GameObject particlesContainer;
        public AvatarAnchorPoint[] anchors;

        [Header("Ghost Settings")]
        [ColorUsage(true, true)] public Color ghostMinColor;
        [ColorUsage(true, true)] public Color ghostMaxColor;
        public float fadeGhostSpeed = 2f;
        public float revealSpeed = 2f;

        public SkinnedMeshRenderer SkinnedMeshRenderer => skinnedMeshRenderer;

        private (string AnchorName, Transform Bone)[] anchorsCache;
        public (string AnchorName, Transform Bone)[] Anchors => anchorsCache ??= CacheAnchors();

        public Transform ArmatureContainer => armatureContainer;
        public GameObject ParticlesContainer => particlesContainer;
        public Color GhostMinColor => ghostMinColor;
        public Color GhostMaxColor => ghostMaxColor;
        public float FadeGhostSpeed => fadeGhostSpeed;
        public float RevealSpeed => revealSpeed;

        private (string AnchorName, Transform Bone)[] CacheAnchors()
        {
            var cachedAnchors = new (string, Transform)[anchors.Length];

            for (var i = 0; i < anchors.Length; i++)
            {
                AvatarAnchorPoint anchorPoint = anchors[i];
                cachedAnchors[i] = (anchorPoint.AnchorName, anchorPoint.Bone);
            }

            return cachedAnchors;
        }

        public void Dispose()
        {
            Destroy(gameObject);
        }
    }
}
