using DCL.Components;
using DCL.Helpers;
using UnityEngine;

namespace DCL
{
    [RequireComponent(typeof(AvatarShape))]
    public class AvatarMovementController : MonoBehaviour, IPoolLifecycleHandler
    {
        const float SPEED_SLOW = 2.0f;
        const float SPEED_FAST = 4.0f;
        const float SPEED_ULTRA_FAST = 8.0f;
        const float SPEED_GRAVITY = 8.0f;
        const float ROTATION_SPEED = 6.25f;
        const float SPEED_EPSILON = 0.0001f;

        Transform avatarTransform
        {
            get
            {
                if (avatarTransformValue == null)
                    avatarTransformValue = GetComponent<AvatarShape>().entity.gameObject.transform;

                return avatarTransformValue;
            }
        }

        Transform avatarTransformValue;

        Vector3 currentPosition
        {
            get { return currentWorldPosition; }

            set
            {
                currentWorldPosition = value;
                avatarTransform.position = PositionUtils.WorldToUnityPosition(currentWorldPosition);
            }
        }

        Vector3 currentWorldPosition = Vector3.zero;

        Quaternion currentRotation
        {
            get { return avatarTransform.rotation; }
            set { avatarTransform.rotation = value; }
        }

        Vector3 targetPosition;
        Quaternion targetRotation;

        float movementSpeed = SPEED_SLOW;

        public void OnPoolGet()
        {
        }

        public void OnPoolRelease()
        {
            avatarTransformValue = null;
            currentWorldPosition = Vector3.zero;
        }

        void OnEnable()
        {
            CommonScriptableObjects.worldOffset.OnChange += OnWorldReposition;
        }

        void OnDisable()
        {
            CommonScriptableObjects.worldOffset.OnChange -= OnWorldReposition;
        }

        void OnWorldReposition(Vector3 current, Vector3 previous)
        {
            avatarTransform.position = PositionUtils.WorldToUnityPosition(currentWorldPosition);
        }

        public void OnTransformChanged(DCLTransform.Model model)
        {
            MoveTo(
                model.position - Vector3.up * DCLCharacterController.i.characterController.height / 2, // To fix the "always flying" avatars bug, We report the chara's centered position but the body hast its pivot at its feet
                model.rotation);
        }

        public void MoveTo(Vector3 position, Quaternion rotation, bool immediate = false)
        {
            if (immediate)
            {
                currentPosition = position;
                avatarTransform.rotation = rotation;
            }

            Vector3 flatEulerRotation = rotation.eulerAngles;
            flatEulerRotation.z = flatEulerRotation.x = 0;
            rotation = Quaternion.Euler(flatEulerRotation);

            targetPosition = position;
            targetRotation = rotation;

            float distance = Vector3.Distance(targetPosition, currentWorldPosition);

            //NOTE(Brian): More distance to goal = faster movement.
            if (distance >= 50)
            {
                this.movementSpeed = float.MaxValue;
            }
            else if (distance >= 3)
            {
                this.movementSpeed = Mathf.Lerp(SPEED_SLOW, SPEED_ULTRA_FAST, (distance - 3) / 10.0f);
            }
            else
            {
                this.movementSpeed = SPEED_SLOW;
            }
        }


        void UpdateLerp(float deltaTime)
        {
            if (Vector3.SqrMagnitude(currentPosition - targetPosition) < SPEED_EPSILON)
            {
                UpdateRotation(deltaTime, targetRotation);
                return;
            }

            //NOTE(Brian): When we update movement we don't update rotation, because the Avatar will face the movement forward vector.
            UpdateMovement(deltaTime);
        }

        private void UpdateRotation(float deltaTime, Quaternion targetRotation)
        {
            currentRotation = Quaternion.Slerp(currentRotation, targetRotation, ROTATION_SPEED * deltaTime);
        }

        private void UpdateMovement(float deltaTime)
        {
            Vector3 flattenedDiff = targetPosition - currentPosition;
            flattenedDiff.y = 0;

            //NOTE(Brian): Avoid Unity error when computing look rotation for 0 magnitude vectors.
            //             Note that this isn't the same as the previous distance check because this
            //             is computed with a flattened vector.
            if (flattenedDiff != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(flattenedDiff, Vector3.up);
                UpdateRotation(deltaTime, lookRotation);
            }

            Vector3 direction = (targetPosition - currentPosition).normalized;
            Vector3 delta = direction * (movementSpeed * deltaTime);

            //NOTE(Brian): We need a separate value for Y movement because the gravity has to be lerped faster.
            delta.y = direction.y * SPEED_GRAVITY * deltaTime;

            //NOTE(Brian): If we overshoot targetPosition we adjust the delta value accordingly.
            if (delta.sqrMagnitude > Vector3.SqrMagnitude(targetPosition - currentPosition))
            {
                delta = targetPosition - currentPosition;
            }

            currentPosition += delta;
        }

        void Update()
        {
            if (avatarTransformValue == null) return;

            UpdateLerp(Time.deltaTime);
        }
    }
}