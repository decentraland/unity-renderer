using DCL.Controllers;
using DCL.ECSComponents;
using DCL.Models;
using UnityEngine;

namespace ECSSystems.Helpers
{
    public static class TransformHelper
    {
        private static readonly ECSTransform reusableTransform = new ECSTransform()
        {
            position = Vector3.zero,
            scale = Vector3.one,
            parentId = SpecialEntityId.SCENE_ROOT_ENTITY,
            rotation = Quaternion.identity
        };

        public static ECSTransform SetTransform(IParcelScene scene, ref Vector3 position,
            ref Quaternion rotation, ref Vector3 worldOffset)
        {
            Vector3Helper.SetInSceneOffset(ref reusableTransform.position, scene, ref position, ref worldOffset);
            QuaternionHelper.Set(ref reusableTransform.rotation, ref rotation);

            return reusableTransform;
        }
    }
}