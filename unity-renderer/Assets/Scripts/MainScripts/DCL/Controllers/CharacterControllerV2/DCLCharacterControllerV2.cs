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
        private readonly InputAction_Hold walkAction;
        private readonly IInputActionMeasurable characterXAxis;
        private readonly IInputActionMeasurable characterYAxis;
        private readonly Vector3Variable cameraForward;
        private readonly Vector3Variable cameraRight;

        private readonly CharacterState characterState;

        private SpeedState speedState;
        private bool jumpButtonPressed;
        private bool jumpTriggered;
        private float xAxis;
        private float yAxis;
        private float xVelocity;
        private float zVelocity;
        private Vector3 velocity;
        private bool isGrounded;
        private float lastJumpHeight;
        private float lastGroundedTime;
        private float lastJumpTime;

        // this is used by SmoothDamp to deaccelerate
        private float xDampSpeed;
        private float yDampSpeed;
        private bool isRunning;
        private bool isWalking;

        public DCLCharacterControllerV2(ICharacterView view, CharacterControllerData data, IInputActionHold jumpAction, IInputActionHold sprintAction, InputAction_Hold walkAction,
            IInputActionMeasurable characterXAxis,
            IInputActionMeasurable characterYAxis, Vector3Variable cameraForward, Vector3Variable cameraRight)
        {
            this.view = view;
            this.data = data; // for testing purposes we are using the raw data
            this.jumpAction = jumpAction;
            this.sprintAction = sprintAction;
            this.walkAction = walkAction;
            this.characterXAxis = characterXAxis;
            this.characterYAxis = characterYAxis;
            this.cameraForward = cameraForward;
            this.cameraRight = cameraRight;

            RegisterInputEvents();
            speedState = SpeedState.JOG;
            characterState = new CharacterState();
        }

        private void RegisterInputEvents()
        {
            //todo: unsubscribe
            jumpAction.OnStarted += _ => ToggleJump(true);
            jumpAction.OnFinished += _ => ToggleJump(false);
            sprintAction.OnStarted += _ => ToggleSprint(true);
            sprintAction.OnFinished += _ => ToggleSprint(false);
            walkAction.OnStarted += _ => ToggleWalk(true);
            walkAction.OnFinished += _ => ToggleWalk(false);

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
            jumpTriggered = active;
            jumpButtonPressed = active;
        }

        private void ToggleSprint(bool active)
        {
            isRunning = active;
        }

        private void ToggleWalk(bool active)
        {
            isWalking = active;
        }

        public void Update(float deltaTime)
        {
            speedState = isRunning ? SpeedState.RUN : isWalking ? SpeedState.WALK : SpeedState.JOG;

            float velocityLimit = GetVelocityLimit();

            CalculateHorizontalVelocity(deltaTime, velocityLimit);
            CalculateVerticalVelocity(deltaTime);

            isGrounded = view.Move(velocity * deltaTime);

            if (isGrounded)
                lastGroundedTime = Time.time;

            // update the state for the animations
            characterState.IsLongJump = lastJumpHeight > data.jogJumpHeight;
            characterState.IsGrounded = isGrounded;
            characterState.IsJumping = !isGrounded && velocity.y > 1f;
            characterState.IsFalling = !isGrounded && velocity.y < -5f;
            characterState.TotalVelocity = velocity;
            characterState.SpeedState = speedState;
            characterState.MaxVelocity = velocityLimit;
        }

        private void CalculateVerticalVelocity(float deltaTime)
        {
            float targetGravity = data.gravity;

            // reset vertical velocity on ground
            if (isGrounded)
            {
                velocity.y = targetGravity * deltaTime;

                // reset jump time to avoid low grav fall without jumping
                lastJumpTime = 0;
            }

            // apply jump impulse
            if (CanJump())
            {
                characterState.Jump();
                lastJumpHeight = GetJumpHeight();
                velocity.y += Mathf.Sqrt(-2 * lastJumpHeight * data.gravity);
                jumpTriggered = false;
                lastGroundedTime = 0; // to avoid double jumping
                lastJumpTime = Time.time;
            }

            // apply gravity factor if jump button is pressed for a long time (higher jump)
            if (jumpButtonPressed && Time.time - lastJumpTime < data.longJumpTime)
                targetGravity = data.gravity * data.longJumpGravityScale;

            // apply gravity
            if (!isGrounded)
                velocity.y += targetGravity * deltaTime;
        }

        private void CalculateHorizontalVelocity(float deltaTime, float velocityLimit)
        {
            CalculateHorizontalInputVelocity(deltaTime, velocityLimit);

            // convert axis velocity to directional vectors using the camera
            var xzPlaneForward = cameraForward.Get();
            var xzPlaneRight = cameraRight.Get();
            xzPlaneForward.y = 0;
            xzPlaneRight.y = 0;

            var targetHorizontal = Vector3.zero;
            targetHorizontal += xzPlaneForward.normalized * zVelocity;
            targetHorizontal += xzPlaneRight.normalized * xVelocity;
            targetHorizontal = Vector3.ClampMagnitude(targetHorizontal, velocityLimit);

            if (targetHorizontal.normalized.sqrMagnitude > 0.1f)
                view.SetForward(targetHorizontal.normalized);

            var horizontalVelocity = velocity;
            horizontalVelocity.y = 0;

            // air control
            if (!isGrounded)
                horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, targetHorizontal, data.airAcceleration * deltaTime);
            else
                horizontalVelocity = targetHorizontal;

            velocity.x = horizontalVelocity.x;
            velocity.z = horizontalVelocity.z;
            characterState.FlatVelocity = horizontalVelocity;
        }

        private bool CanJump()
        {
            bool wasJustGrounded = Time.time - lastGroundedTime < data.jumpGraceTime;
            return jumpButtonPressed && (isGrounded || wasJustGrounded);
        }

        private void CalculateHorizontalInputVelocity(float deltaTime, float velocityLimit)
        {
            // To accelerate we use a lineal acceleration
            // Before accelerating whe convert the sign of the current velocity to avoid de-accelerating when changing directions (AD-AD-ing), this improves the responsiveness by a lot
            // To de accelerate we damp using times

            float targetX = xAxis * velocityLimit;
            float targetY = yAxis * velocityLimit;

            if (Mathf.Abs(xAxis) > 0)
            {
                xVelocity = Mathf.Sign(targetX) * Mathf.Abs(xVelocity);
                xVelocity = Mathf.MoveTowards(xVelocity, targetX, data.acceleration * deltaTime);
            }
            else
                xVelocity = Mathf.SmoothDamp(xVelocity, targetX, ref xDampSpeed, data.stopTimeSec);


            if (Mathf.Abs(yAxis) > 0)
            {
                zVelocity = Mathf.Sign(targetY) * Mathf.Abs(zVelocity);
                zVelocity = Mathf.MoveTowards(zVelocity, targetY, data.acceleration * deltaTime);
            }
            else
                zVelocity = Mathf.SmoothDamp(zVelocity, targetY, ref yDampSpeed, data.stopTimeSec);
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

        private float GetJumpHeight()
        {
            return speedState switch
                   {
                       SpeedState.WALK => data.walkJumpHeight,
                       SpeedState.JOG => data.jogJumpHeight,
                       SpeedState.RUN => data.runJumpHeight,
                       _ => throw new ArgumentOutOfRangeException(),
                   };
        }

        public CharacterState GetCharacterState() =>
            characterState;
    }
}
