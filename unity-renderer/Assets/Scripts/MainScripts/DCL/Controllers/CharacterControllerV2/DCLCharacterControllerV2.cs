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
        private bool wantsToJump;
        private float xAxis;
        private float yAxis;
        private float xVelocity;
        private float zVelocity;
        private Vector3 velocity;
        private bool isGrounded;
        private float lastJumpHeight;
        private float lastGroundedTime;

        // this is used by SmoothDamp to deaccelerate
        private float xDampSpeed;
        private float yDampSpeed;

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
            wantsToJump = active;
        }

        private void ToggleSprint(bool active)
        {
            speedState = active ? SpeedState.RUN : SpeedState.JOG;
        }

        private void ToggleWalk(bool active)
        {
            speedState = active ? SpeedState.WALK : SpeedState.JOG;
        }

        public void Update(float deltaTime)
        {
            float velocityLimit = GetVelocityLimit();

            CalculateHorizontalInputVelocity(deltaTime, velocityLimit);

            var horizontalVelocity = velocity;
            horizontalVelocity.y = 0;

            var targetHorizontal = Vector3.zero;

            var xzPlaneForward = Vector3.Scale(cameraForward.Get(), new Vector3(1, 0, 1));
            var xzPlaneRight = Vector3.Scale(cameraRight.Get(), new Vector3(1, 0, 1));

            targetHorizontal += xzPlaneForward * zVelocity;
            targetHorizontal += xzPlaneRight * xVelocity;

            targetHorizontal = Vector3.ClampMagnitude(targetHorizontal, velocityLimit);

            if (targetHorizontal.normalized.sqrMagnitude > 0.1f)
                view.SetForward(targetHorizontal.normalized);

            if (!isGrounded)
                horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, targetHorizontal, data.airAcceleration * deltaTime);
            else
                horizontalVelocity = targetHorizontal;

            velocity.x = horizontalVelocity.x;
            velocity.z = horizontalVelocity.z;

            if (isGrounded)
                velocity.y = data.gravity * deltaTime;
            else
                velocity.y += data.gravity * deltaTime;

            if (CanJump())
            {
                characterState.Jump();
                lastJumpHeight = GetJumpHeight();
                velocity.y += Mathf.Sqrt(-2 * lastJumpHeight * data.gravity);
                wantsToJump = false;
                lastGroundedTime = 0; // to avoid double jumping
            }

            isGrounded = view.Move(velocity * deltaTime);

            if (isGrounded)
                lastGroundedTime = Time.time;

            characterState.IsLongJump = lastJumpHeight > data.jogJumpHeight; // replace this with jumpPad strength or something
            characterState.IsGrounded = isGrounded;
            characterState.IsJumping = !isGrounded && velocity.y > 1f;
            characterState.IsFalling = !isGrounded && velocity.y < -5f;
            characterState.FlatVelocity = horizontalVelocity;
            characterState.TotalVelocity = velocity;
            characterState.SpeedState = speedState;
            characterState.MaxVelocity = velocityLimit;
        }

        private bool CanJump()
        {
            bool wasJustGrounded = Time.time - lastGroundedTime < data.jumpGraceTime;
            return wantsToJump && (isGrounded || wasJustGrounded);
        }

        private void CalculateHorizontalInputVelocity(float deltaTime, float velocityLimit)
        {
            // To accelerate we use a lineal acceleration
            // To de accelerate we damp using times

            if (Mathf.Abs(xAxis) > 0)
                xVelocity = Mathf.MoveTowards(xVelocity, xAxis * velocityLimit, data.acceleration * deltaTime);
            else
                xVelocity = Mathf.SmoothDamp(xVelocity, xAxis * velocityLimit, ref xDampSpeed, data.stopTimeSec);

            if (Mathf.Abs(yAxis) > 0)
                zVelocity = Mathf.MoveTowards(zVelocity, yAxis * velocityLimit, data.acceleration * deltaTime);
            else
                zVelocity = Mathf.SmoothDamp(zVelocity, yAxis * velocityLimit, ref yDampSpeed, data.stopTimeSec);
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
