using System;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace MainScripts.DCL.Controllers.CharacterControllerV2.IK
{
    [Serializable]
    public struct ExtractTransformConstraintData : IAnimationJobData
    {
        [SyncSceneToStream] public Transform bone;

        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;

        public bool IsValid() =>
            bone != null;

        public void SetDefaultValues()
        {
            this.bone = null;

            this.position = Vector3.zero;
            this.rotation = Quaternion.identity;
            this.scale = Vector3.zero;
        }
    }
}
