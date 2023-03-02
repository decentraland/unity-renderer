using UnityEngine;

namespace AvatarSystem
{
    public interface IAvatarMovementController
    {
        /// <summary>
        /// This will change the interpolation of the avatar position
        /// </summary>
        /// <param name="secondsBetweenUpdates"></param>
        void SetMovementLerpWait(float secondsBetweenUpdates);

        /// <summary>
        /// This will set the transform of the avatar
        /// </summary>
        /// <param name="avatarTransform"></param>
        void SetAvatarTransform(Transform avatarTransform);
        
        /// <summary>
        /// This will report the change of the transform, so it can be reported to kernel accordingly
        /// </summary>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <param name="inmediate"></param>
        void OnTransformChanged(in Vector3 position, in Quaternion rotation, bool inmediate);
    }
}