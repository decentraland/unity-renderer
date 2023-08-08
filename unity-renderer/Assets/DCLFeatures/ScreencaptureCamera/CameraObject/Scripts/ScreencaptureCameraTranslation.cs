using DCL;
using UnityEngine;

namespace DCLFeatures.ScreencaptureCamera.CameraObject
{
    public class ScreencaptureCameraTranslation
    {
        private readonly CharacterController characterController;
        private readonly float moveSpeed;
        private readonly float maxDistanceFromPlayer;
        private readonly Transform transform;
        private readonly TranslationInputSchema input;

        private Vector3 currentMoveVector = Vector3.zero;
        private Vector3 smoothedMoveVector = Vector3.zero;

        public ScreencaptureCameraTranslation(CharacterController characterController, float moveSpeed, float maxDistanceFromPlayer, TranslationInputSchema input)
        {
            this.characterController = characterController;
            this.moveSpeed = moveSpeed;
            this.maxDistanceFromPlayer = maxDistanceFromPlayer;
            this.input = input;

            transform = characterController.transform;
        }

        public void Translate(float deltaTime, float damping, float max)
        {
            currentMoveVector = InputToMoveVector(deltaTime);
            smoothedMoveVector = Vector3.Lerp(smoothedMoveVector, currentMoveVector, deltaTime * damping);
            smoothedMoveVector = Vector3.ClampMagnitude(smoothedMoveVector, max * deltaTime);

            characterController.Move(
                RestrictedMovementBySemiSphere(smoothedMoveVector));
        }

        private Vector3 InputToMoveVector(float deltaTime)
        {
            Vector3 forward = transform.forward.normalized * input.yAxis.GetValue();
            Vector3 horizontal = transform.right.normalized * input.xAxis.GetValue();

            float verticalDirection = input.upAction.isOn ? 1 :
                input.downAction.isOn ? -1 : 0f;

            Vector3 vertical = transform.up.normalized * verticalDirection;

            return (forward + horizontal + vertical) * (moveSpeed * deltaTime);
        }

        private Vector3 RestrictedMovementBySemiSphere(Vector3 movementVector)
        {
            if (characterController.transform.position.y + movementVector.y <= 0f)
                movementVector.y = 0f;

            Vector3 playerPosition = DataStore.i.player.playerUnityPosition.Get();
            Vector3 desiredCameraPosition = characterController.transform.position + movementVector;

            float distanceFromPlayer = Vector3.Distance(desiredCameraPosition, playerPosition);

            if (distanceFromPlayer > maxDistanceFromPlayer)
            {
                // If the distance is greater than the allowed radius, correct the movement vector
                Vector3 directionFromPlayer = (desiredCameraPosition - playerPosition).normalized;
                desiredCameraPosition = playerPosition + (directionFromPlayer * maxDistanceFromPlayer);
                movementVector = desiredCameraPosition - characterController.transform.position;
            }

            return movementVector;
        }
    }
}
