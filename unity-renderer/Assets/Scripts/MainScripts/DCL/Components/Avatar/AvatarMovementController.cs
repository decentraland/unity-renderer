using AvatarSystem;
using DCL.Components;
using DCL.Helpers;
using System;
using UnityEngine;

namespace DCL
{
    public class AvatarMovementController : MonoBehaviour, IPoolLifecycleHandler, IAvatarMovementController
    {
        private const float SPEED_SLOW = 2.0f;
        private const float SPEED_FAST = 4.0f;
        private const float SPEED_ULTRA_FAST = 8.0f;
        private const float SPEED_GRAVITY = 8.0f;
        private const float ROTATION_SPEED = 6.25f;
        private const float SPEED_EPSILON = 0.0001f;
        private float movementSpeed = SPEED_SLOW;

        private Transform avatarTransformValue;

        private Quaternion targetRotation;
        private Vector3 currentWorldPosition = Vector3.zero;
        private Vector3 targetPosition;

        private float movementLerpWait = 0f;
        private float movementLerpWaitCounter = 0f;


        private Transform AvatarTransform
        {
            get
            {
                if (avatarTransformValue == null)
                {
                    avatarTransformValue = GetComponent<AvatarShape>().entity.gameObject.transform;
                    enabled = true;
                }

                return avatarTransformValue;
            }
            set
            {
                avatarTransformValue = value;

                if (value == null)
                    enabled = false;
            }
        }

        private Vector3 CurrentPosition
        {
            get { return currentWorldPosition; }
            set
            {
                currentWorldPosition = value;
                Vector3 newPosition = PositionUtils.WorldToUnityPosition(currentWorldPosition);
                if (float.IsNaN(newPosition.x) || float.IsInfinity(newPosition.x) ||
                    float.IsNaN(newPosition.z) || float.IsInfinity(newPosition.z) ||
                    float.IsNaN(newPosition.y) || float.IsInfinity(newPosition.y))
                    return;

                AvatarTransform.position = newPosition;
            }
        }

        private Quaternion CurrentRotation
        {
            get { return AvatarTransform.rotation; }
            set { AvatarTransform.rotation = value; }
        }

        public void OnPoolGet() { }

        public void OnPoolRelease()
        {
            AvatarTransform = null;

            currentWorldPosition = Vector3.zero;
        }

        public void SetAvatarTransform(Transform avatarTransform)
        {
            AvatarTransform = avatarTransform;
        }

        private void OnEnable() { CommonScriptableObjects.worldOffset.OnChange += OnWorldReposition; }

        private void OnDisable() { CommonScriptableObjects.worldOffset.OnChange -= OnWorldReposition; }

        private void OnWorldReposition(Vector3 current, Vector3 previous)
        {
            AvatarTransform.position = PositionUtils.WorldToUnityPosition(currentWorldPosition);
        }

        public void OnTransformChanged(object model)
        {
            DCLTransform.Model transformModel = (DCLTransform.Model)model;
            OnTransformChanged(transformModel.position, transformModel.rotation, false);
        }

        public void OnTransformChanged(in Vector3 position, in Quaternion rotation, bool inmediate)
        {
            float characterMinHeight = DCLCharacterController.i.characterController.height * 0.5f;

            MoveTo(
                new Vector3(position.x, Math.Max(position.y - characterMinHeight, -characterMinHeight), position.z), // To fix the "always flying" avatars issue, We report the chara's centered position but the body hast its pivot at its feet
                rotation,
                inmediate);
        }

        public void MoveTo(Vector3 position, Quaternion rotation, bool immediate = false)
        {
            if (immediate)
            {
                CurrentPosition = position;
                AvatarTransform.rotation = rotation;
            }

            Vector3 flatEulerRotation = rotation.eulerAngles;
            flatEulerRotation.z = flatEulerRotation.x = 0;
            rotation = Quaternion.Euler(flatEulerRotation);

            targetPosition = position;
            targetRotation = rotation;

            float distance = Vector3.Distance(targetPosition, currentWorldPosition);

            if (distance >= 50)
                movementSpeed = float.MaxValue;
            else if (distance >= 3)
                movementSpeed = Mathf.Lerp(SPEED_SLOW, SPEED_ULTRA_FAST, (distance - 3) / 10.0f);
            else
                movementSpeed = SPEED_SLOW;
        }

        void UpdateLerp(float deltaTime)
        {
            if (Vector3.SqrMagnitude(CurrentPosition - targetPosition) < SPEED_EPSILON)
            {
                UpdateRotation(deltaTime, targetRotation);
                return;
            }

            //NOTE(Brian): When we update movement we don't update rotation, because the Avatar will face the movement forward vector.
            UpdateMovement(deltaTime);
        }

        private void UpdateRotation(float deltaTime, Quaternion targetRotation)
        {
            CurrentRotation = Quaternion.Slerp(CurrentRotation, targetRotation, ROTATION_SPEED * deltaTime);
        }

        private void UpdateMovement(float deltaTime)
        {
            Vector3 flattenedDiff = targetPosition - CurrentPosition;
            flattenedDiff.y = 0;

            //NOTE(Brian): Avoid Unity error when computing look rotation for 0 magnitude vectors.
            //             Note that this isn't the same as the previous distance check because this
            //             is computed with a flattened vector.
            if (flattenedDiff != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(flattenedDiff, Vector3.up);
                UpdateRotation(deltaTime, lookRotation);
            }

            Vector3 direction = (targetPosition - CurrentPosition).normalized;
            Vector3 delta = direction * (movementSpeed * deltaTime);

            //NOTE(Brian): We need a separate value for Y movement because the gravity has to be lerped faster.
            delta.y = direction.y * SPEED_GRAVITY * deltaTime;

            //NOTE(Brian): If we overshoot targetPosition we adjust the delta value accordingly.
            if (delta.sqrMagnitude > Vector3.SqrMagnitude(targetPosition - CurrentPosition))
            {
                delta = targetPosition - CurrentPosition;
            }

            CurrentPosition += delta;
        }

        private void Update()
        {
            movementLerpWaitCounter += Time.deltaTime;
            if (movementLerpWaitCounter >= movementLerpWait)
            {
                UpdateLerp(movementLerpWaitCounter);
                movementLerpWaitCounter = 0f;
            }
        }

        public void SetMovementLerpWait(float secondsBetweenUpdates) { movementLerpWait = secondsBetweenUpdates; }
    }
}
