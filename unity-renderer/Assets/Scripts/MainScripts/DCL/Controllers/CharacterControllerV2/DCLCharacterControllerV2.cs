using DCL;
using System;
using UnityEngine;

namespace MainScripts.DCL.Controllers.CharacterControllerV2
{
    public enum SpeedState
    {
        WALK,
        JOG,
        RUN
    }

    internal class DCLCharacterControllerV2
    {
        private readonly ICharacterView view;
        private readonly CharacterControllerData data;
        private readonly IInputActionHold jumpAction;
        private readonly IInputActionHold sprintAction;
        private readonly IInputActionMeasurable characterXAxis;
        private readonly IInputActionMeasurable characterYAxis;
        private readonly Vector3Variable cameraForward;
        private readonly Vector3Variable cameraRight;

        private SpeedState speedState;
        private bool isJumping;
        private float xAxis;
        private float yAxis;
        private float xVelocity;
        private float zVelocity;
        private Vector3 velocity;
        private bool isGrounded;

        public DCLCharacterControllerV2(ICharacterView view, CharacterControllerData data, IInputActionHold jumpAction, IInputActionHold sprintAction, IInputActionMeasurable characterXAxis,
            IInputActionMeasurable characterYAxis, Vector3Variable cameraForward, Vector3Variable cameraRight)
        {
            this.view = view;
            this.data = data; // for testing purposes we are using the raw data
            this.jumpAction = jumpAction;
            this.sprintAction = sprintAction;
            this.characterXAxis = characterXAxis;
            this.characterYAxis = characterYAxis;
            this.cameraForward = cameraForward;
            this.cameraRight = cameraRight;

            RegisterInputEvents();
            speedState = SpeedState.JOG;
        }

        private void RegisterInputEvents()
        {
            jumpAction.OnStarted += _ => ToggleJump(true);
            jumpAction.OnFinished += _ => ToggleJump(false);
            sprintAction.OnStarted += _ => ToggleSprint(true);
            sprintAction.OnFinished += _ => ToggleSprint(false);

            characterXAxis.OnValueChanged += OnHorizontalInput;
            characterYAxis.OnValueChanged += OnVerticalInput;
        }

        private void OnVerticalInput(DCLAction_Measurable action, float value)
        {
            yAxis = value;
        }

        private void OnHorizontalInput(DCLAction_Measurable action, float value)
        {
            xAxis = value;
        }

        private void ToggleJump(bool active)
        {
            isJumping = active;
        }

        private void ToggleSprint(bool active)
        {
            speedState = active ? SpeedState.RUN : SpeedState.JOG;
        }

        public void Update(float deltaTime)
        {
            float velocityLimit = GetVelocityLimit();

            float targetHorizontalVelocity = xAxis * velocityLimit;
            xVelocity = Mathf.MoveTowards(xVelocity, targetHorizontalVelocity, data.acceleration * deltaTime);

            float targetForwardVelocity = yAxis * velocityLimit;
            zVelocity = Mathf.MoveTowards(zVelocity, targetForwardVelocity, data.acceleration * deltaTime);

            var horizontalVelocity = velocity;
            horizontalVelocity.y = 0;

            var targetHorizontal = Vector3.zero;

            var xzPlaneForward = Vector3.Scale(cameraForward.Get(), new Vector3(1, 0, 1));
            var xzPlaneRight = Vector3.Scale(cameraRight.Get(), new Vector3(1, 0, 1));

            targetHorizontal += xzPlaneForward * zVelocity;
            targetHorizontal += xzPlaneRight * xVelocity;

            if (targetHorizontal.sqrMagnitude > float.Epsilon)
            {
                view.SetForward(targetHorizontal.normalized);
            }

            if (!isGrounded)
                horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, Vector3.ClampMagnitude(targetHorizontal, velocityLimit), data.airAcceleration * deltaTime);
            else
                horizontalVelocity = targetHorizontal;

            velocity.x = horizontalVelocity.x;
            velocity.z = horizontalVelocity.z;
            velocity.y += data.gravity * deltaTime;

            if (isJumping && isGrounded)
            {
                velocity.y += Mathf.Sqrt(-2 * data.jumpHeight * data.gravity);
                isJumping = false;
            }

            if (velocity.sqrMagnitude > float.Epsilon)
            {
                var collisionFlags = view.Move(velocity * deltaTime);

                if ((collisionFlags & CollisionFlags.Below) != 0)
                {
                    velocity.y = 0;
                    isGrounded = true;
                }
                else
                {
                    isGrounded = false;
                }
            }
        }

        private float GetVelocityLimit()
        {
            return speedState switch
                   {
                       SpeedState.WALK => data.walkSpeed,
                       SpeedState.JOG => data.jogSpeed,
                       SpeedState.RUN => data.runSpeed,
                       _ => throw new ArgumentOutOfRangeException(),
                   };
        }
    }
}
