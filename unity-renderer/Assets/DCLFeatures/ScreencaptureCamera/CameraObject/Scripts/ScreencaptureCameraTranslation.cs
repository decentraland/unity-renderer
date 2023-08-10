using DCL;
using UnityEngine;

namespace DCLFeatures.ScreencaptureCamera.CameraObject
{
    public class ScreencaptureCameraTranslation
    {
        private CharacterController characterController;
        private Transform transform;
        private readonly TranslationInputSchema input;

        private Vector3 currentMoveVector = Vector3.zero;
        private Vector3 smoothedMoveVector = Vector3.zero;

        public ScreencaptureCameraTranslation(CharacterController characterController, TranslationInputSchema input)
        {
            this.characterController = characterController;
            this.input = input;
        }

        public void Translate(Transform target, float deltaTime, float moveSpeed, float damping, float maxPerFrame, float maxDistanceFromPlayer)
        {
            characterController = target.GetComponent<CharacterController>();
            transform = target;
            currentMoveVector = GetMoveVectorFromInput(deltaTime, moveSpeed);
            // smoothedMoveVector = Vector3.Lerp(smoothedMoveVector, currentMoveVector, deltaTime * damping);
            // smoothedMoveVector = Vector3.ClampMagnitude(smoothedMoveVector, maxPerFrame * deltaTime);
            // target.position += currentMoveVector;
            characterController.Move(
                RestrictedMovementBySemiSphere(currentMoveVector, maxDistanceFromPlayer));
        }

        private Vector3 GetMoveVectorFromInput(float deltaTime, float moveSpeed)
        {
            Vector3 forward = transform.forward.normalized * input.yAxis.GetValue();
            Vector3 horizontal = transform.right.normalized * input.xAxis.GetValue();

            float verticalDirection = input.upAction.isOn ? 1 :
                input.downAction.isOn ? -1 : 0f;

            Vector3 vertical = transform.up.normalized * verticalDirection;

            return (forward + horizontal + vertical) * (moveSpeed * deltaTime);
        }

        private Vector3 RestrictedMovementBySemiSphere(Vector3 movementVector, float maxDistanceFromPlayer)
        {
            if (characterController.transform.position.y + movementVector.y <= 0f)
                movementVector.y = 0f;

            Vector3 playerPosition = DataStore.i.player.playerUnityPosition.Get();
            Vector3 desiredCameraPosition = characterController.transform.position + movementVector;

            float distanceFromPlayer = Vector3.Distance(desiredCameraPosition, playerPosition);

            // If the distance is greater than the allowed radius, correct the movement vector
            if (distanceFromPlayer > maxDistanceFromPlayer)
            {
                Vector3 directionFromPlayer = (desiredCameraPosition - playerPosition).normalized;
                desiredCameraPosition = playerPosition + (directionFromPlayer * maxDistanceFromPlayer);
                movementVector = desiredCameraPosition - characterController.transform.position;
            }

            return movementVector;
        }
    }
}
