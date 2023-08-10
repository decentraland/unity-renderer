using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace MainScripts.DCL.Controllers.CharacterControllerV2.IK
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Animation Rigging/Extract Transform Constraint")]
    public class ExtractTransformConstraint : RigConstraint<
        ExtractTransformConstraintJob,
        ExtractTransformConstraintData,
        ExtractTransformConstraintJobBinder>
    {

    }
}
