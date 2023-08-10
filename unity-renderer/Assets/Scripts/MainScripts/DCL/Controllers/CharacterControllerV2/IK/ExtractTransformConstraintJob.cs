using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Animations.Rigging;

namespace MainScripts.DCL.Controllers.CharacterControllerV2.IK
{
    public struct ExtractTransformConstraintJob : IWeightedAnimationJob
    {
        public ReadWriteTransformHandle bone;

        public FloatProperty jobWeight { get; set; }

        public Vector3Property position;
        public Vector4Property rotation;
        public Vector3Property scale;

        public void ProcessRootMotion(AnimationStream stream) { }

        public void ProcessAnimation(AnimationStream stream)
        {
            AnimationRuntimeUtils.PassThrough(stream, this.bone);

            Vector3 pos = this.bone.GetLocalPosition(stream);
            Quaternion rot = this.bone.GetLocalRotation(stream);
            Vector3 sc = this.bone.GetLocalScale(stream);

            this.position.Set(stream, pos);
            this.scale.Set(stream, sc);
            this.rotation.Set(stream, new Vector4(rot.x, rot.y, rot.z, rot.w));
        }
    }
}
