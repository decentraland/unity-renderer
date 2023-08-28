using DCL;
using UnityEngine;

namespace DCLFeatures.ScreencaptureCamera.CameraObject
{
    public class ScreencaptureCameraTranslation
    {
        private readonly TranslationInputSchema input;

        private Vector3 moveVector = Vector3.zero;

        public ScreencaptureCameraTranslation(TranslationInputSchema input)
        {
            this.input = input;
        }

        public void Translate(CharacterController target, float moveSpeed, float maxDistanceFromPlayer, float deltaTime)
        {
            moveVector = GetMoveVectorFromInput(target.transform, moveSpeed, deltaTime);
            target.Move(RestrictedMovementBySemiSphere(target.transform, moveVector, maxDistanceFromPlayer));
        }

        private Vector3 GetMoveVectorFromInput(Transform target, float moveSpeed, float deltaTime)
        {
            Vector3 forward = target.forward.normalized * input.YAxis.GetValue();
            Vector3 horizontal = target.right.normalized * input.XAxis.GetValue();

            float verticalDirection = input.UpAction.isOn ? 1 :
                input.DownAction.isOn ? -1 : 0f;

            Vector3 vertical = target.up.normalized * verticalDirection;

            return (forward + horizontal + vertical) * (moveSpeed * deltaTime);
        }

        private static Vector3 RestrictedMovementBySemiSphere(Transform target, Vector3 movementVector, float maxDistanceFromPlayer)
        {
            if (target.position.y + movementVector.y <= 0f)
                movementVector.y = 0f;

            Vector3 playerPosition = DataStore.i.player.playerUnityPosition.Get();
            Vector3 desiredCameraPosition = target.position + movementVector;

            float distanceFromPlayer = Vector3.Distance(desiredCameraPosition, playerPosition);

            // If the distance is greater than the allowed radius, correct the movement vector
            if (distanceFromPlayer > maxDistanceFromPlayer)
            {
                Vector3 directionFromPlayer = (desiredCameraPosition - playerPosition).normalized;
                desiredCameraPosition = playerPosition + (directionFromPlayer * maxDistanceFromPlayer);
                movementVector = desiredCameraPosition - target.position;
            }

            return movementVector;
        }
    }
}
