using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace MainScripts.DCL.Controllers.CharacterControllerV2.IK
{
    public class ExtractTransformConstraintJobBinder : AnimationJobBinder<
        ExtractTransformConstraintJob,
        ExtractTransformConstraintData>
    {
        public override ExtractTransformConstraintJob Create(Animator animator,
            ref ExtractTransformConstraintData data, Component component) =>
            new()
            {
                bone = ReadWriteTransformHandle.Bind(animator, data.bone),
                position = Vector3Property.Bind(animator, component, "m_Data." + nameof(data.position)),
                rotation = Vector4Property.Bind(animator, component, "m_Data." + nameof(data.rotation)),
                scale = Vector3Property.Bind(animator, component, "m_Data." + nameof(data.scale)),
            };

        public override void Destroy(ExtractTransformConstraintJob job)
        { }
    }
}
