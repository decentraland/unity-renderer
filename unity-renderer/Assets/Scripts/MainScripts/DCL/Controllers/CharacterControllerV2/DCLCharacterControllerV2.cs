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
        private float accelerationWeight;

        private Vector3 externalImpulse;
        private Vector3 externalVelocity;

        // this is used by SmoothDamp to deaccelerate
        private float xDampSpeed;
        private float yDampSpeed;
        private bool isRunning;
        private bool isWalking;
        private float currentAcceleration;
        private float velocityLimit;
        private bool isStunned;
        private float lastStunnedTime;
        private readonly (Vector3 center, float radius, float skinWidth, float height) characterControllerSettings;
        private RaycastHit sphereCastHitInfo;
        private bool isLongFall;

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
            velocityLimit = GetVelocityLimit();
            characterControllerSettings = view.GetCharacterControllerSettings();
        }

        private void RegisterInputEvents()
        {
            //todo: unsubscribe
            jumpAction.OnStarted += _ => ToggleJump(true);
            jumpAction.OnFinished += _ => ToggleJump(false);
            sprintAction.OnStarted += _ => ToggleSprint(true);
            sprintAction.OnFinished += _ => ToggleSprint(false);
            walkAction.OnStarted += _ => ToggleWalk();

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

        private void ToggleWalk()
        {
            isWalking = !isWalking;
        }

        public void Update(float deltaTime)
        {
            UpdateCheatKeys();

            if (isGrounded)
                speedState = isRunning ? SpeedState.RUN :
                    isWalking ? SpeedState.WALK : SpeedState.JOG;

            float targetVelocity = GetVelocityLimit();
            velocityLimit = Mathf.MoveTowards(velocityLimit, targetVelocity, Mathf.Max(velocityLimit, targetVelocity) * 2 * Time.deltaTime);

            CalculateHorizontalVelocity(deltaTime, velocityLimit);
            CalculateVerticalVelocity(deltaTime);

            ApplyDragToVelocity();

            Vector3 finalVelocity = velocity;
            finalVelocity += externalImpulse;
            finalVelocity += externalVelocity;

            Vector3 deltaPosition;

            Vector3 slope = GetSlopeModifier();

            Vector3 slopeDelta = slope * (data.slipSpeedMultiplier * Time.deltaTime);
            Vector3 velocityDelta = finalVelocity * deltaTime;
            (isGrounded, deltaPosition) = view.Move(velocityDelta + slopeDelta);

            ApplyDragToImpulse();

            if (isGrounded)
                lastGroundedTime = Time.time;

            if (!isLongFall)
                isLongFall = finalVelocity.y < -12;

            if (isGrounded && isLongFall)
            {
                lastStunnedTime = Time.time;
                isStunned = true;
                isLongFall = false;
            }

            // update the state for the animations
            characterState.IsLongJump = finalVelocity.y > (data.jogJumpHeight * 3 * data.jumpGravityFactor);

            characterState.IsLongFall = isLongFall;
            characterState.IsGrounded = isGrounded;
            characterState.IsJumping = !isGrounded && (finalVelocity.y > 5f || characterState.IsLongJump);
            characterState.IsFalling = finalVelocity.y < -7f;
            characterState.TotalVelocity = deltaPosition / deltaTime;
            characterState.SpeedState = speedState;
            characterState.MaxVelocity = velocityLimit;
            characterState.ExternalImpulse = externalImpulse;
            characterState.ExternalVelocity = externalVelocity;
            characterState.currentAcceleration = currentAcceleration;
        }

        private Vector3 GetSlopeModifier()
        {
            var settings = characterControllerSettings;

            Vector3 currentPosition = view.GetPosition();

            if (Physics.SphereCast(currentPosition,
                    settings.radius, Vector3.down, out sphereCastHitInfo,
                    1, Physics.AllLayers))
            {
                Vector3 relativeHitPoint = sphereCastHitInfo.point - (currentPosition + settings.center);
                //Debug.DrawLine(sphereCastHitInfo.point, sphereCastHitInfo.point + (sphereCastHitInfo.normal * 5), Color.red, 0.15f);

                relativeHitPoint.y = 0;

                if (relativeHitPoint.magnitude > data.noSlipDistance)
                {
                    Debug.DrawLine(sphereCastHitInfo.point, sphereCastHitInfo.point + relativeHitPoint , Color.green, 0.15f);
                    return -relativeHitPoint;
                }

                Debug.DrawLine(sphereCastHitInfo.point, sphereCastHitInfo.point + relativeHitPoint , Color.red, 0.15f);
            }

            return Vector3.zero;
        }

        private void ApplyDragToVelocity()
        {
            var tempVelocity = Flat(velocity);
            tempVelocity = ApplyDrag(tempVelocity);
            tempVelocity.y = velocity.y;
            velocity = tempVelocity;
        }

        private void UpdateCheatKeys()
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                Vector3 impulse = cameraForward.Get() * data.jumpPadForce;
                if (impulse.y < 0) impulse.y = 0;
                ApplyExternalImpulse(impulse);
            }

            if (Input.GetKey(KeyCode.E))
                externalVelocity = Vector3.right * 5;
            else
                externalVelocity = Vector3.zero;
        }

        private void ApplyExternalImpulse(Vector3 impulse)
        {
            externalImpulse = impulse;
            view.SetForward(FlatNormal(externalImpulse));
            velocity.y = 0;
        }

        private void ApplyDragToImpulse()
        {
            if (isGrounded)
                externalImpulse.y = 0;

            externalImpulse = ApplyDrag(externalImpulse);

            if (externalImpulse.magnitude < data.minImpulse)
                externalImpulse = Vector3.zero;
        }

        private Vector3 ApplyDrag(Vector3 vector)
        {
            float velocityMagnitude = vector.magnitude;
            float drag = isGrounded ? data.groundDrag : data.airDrag;
            float dragMagnitude = drag * velocityMagnitude * velocityMagnitude;
            Vector3 dragDirection = -vector.normalized;
            Vector3 dragForce = dragDirection * dragMagnitude;
            vector += dragForce * Time.deltaTime;
            return vector;
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

                velocity.y += Mathf.Sqrt(-2 * lastJumpHeight * (data.gravity * data.jumpGravityFactor));

                jumpTriggered = false;
                lastGroundedTime = 0; // to avoid double jumping
                lastJumpTime = Time.time;
            }

            // apply gravity factor if jump button is pressed for a long time (higher jump)
            if (jumpButtonPressed && Time.time - lastJumpTime < data.longJumpTime)
                targetGravity = data.gravity * data.longJumpGravityScale;

            if (velocity.y > 0)
                targetGravity *= data.jumpGravityFactor;

            // apply gravity
            if (!isGrounded)
                velocity.y += targetGravity * deltaTime;
        }

        private void CalculateHorizontalVelocity(float deltaTime, float velocityLimit)
        {
            if (isStunned && Time.time - lastStunnedTime < data.longFallStunTime)
            {
                velocity.x = 0;
                velocity.z = 0;
                return;
            }

            isStunned = false;
            lastStunnedTime = 0;

            CalculateHorizontalInputVelocity(deltaTime, velocityLimit);

            // convert axis velocity to directional vectors using the camera
            var targetHorizontal = Vector3.zero;
            targetHorizontal += FlatNormal(cameraForward.Get()) * zVelocity;
            targetHorizontal += FlatNormal(cameraRight.Get()) * xVelocity;

            var impulse = Flat(externalImpulse);

            // velocity limit is based on current impulse, so we cannot move if impulse is too big
            targetHorizontal = Vector3.ClampMagnitude(targetHorizontal, Mathf.Max(velocityLimit - impulse.magnitude, 0));

            if (targetHorizontal.normalized.sqrMagnitude > 0.1f && (Mathf.Abs(xAxis) > 0 || Mathf.Abs(yAxis) > 0))
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
        }

        private Vector3 FlatNormal(Vector3 vector)
        {
            vector = Flat(vector);
            return vector.normalized;
        }

        private Vector3 Flat(Vector3 vector)
        {
            vector.y = 0;
            return vector;
        }

        private bool CanJump()
        {
            bool wasJustGrounded = Time.time - lastGroundedTime < data.jumpGraceTime;
            return jumpTriggered && (isGrounded || wasJustGrounded);
        }

        private void CalculateHorizontalInputVelocity(float deltaTime, float velocityLimit)
        {
            // first we calculate the acceleration curve
            currentAcceleration = data.acceleration;

            int targetAccelerationWeight = Mathf.Abs(xAxis) > 0 || Mathf.Abs(yAxis) > 0 ? 1 : 0;
            accelerationWeight = Mathf.MoveTowards(accelerationWeight, targetAccelerationWeight, Time.deltaTime / data.accelerationTime);
            currentAcceleration = Mathf.Lerp(data.acceleration, data.maxAcceleration, data.accelerationCurve.Evaluate(accelerationWeight));

            // To accelerate we use a lineal acceleration
            // Before accelerating whe convert the sign of the current velocity to avoid de-accelerating when changing directions (AD-AD-ing), this improves the responsiveness by a lot
            // To de accelerate we damp using times

            float targetX = xAxis * velocityLimit;
            float targetY = yAxis * velocityLimit;

            if (Mathf.Abs(xAxis) > 0)
            {
                xVelocity = Mathf.Sign(targetX) * Mathf.Abs(xVelocity);
                xVelocity = Mathf.MoveTowards(xVelocity, targetX, currentAcceleration * deltaTime);
            }
            else
                xVelocity = Mathf.SmoothDamp(xVelocity, targetX, ref xDampSpeed, data.stopTimeSec);

            if (Mathf.Abs(yAxis) > 0)
            {
                zVelocity = Mathf.Sign(targetY) * Mathf.Abs(zVelocity);
                zVelocity = Mathf.MoveTowards(zVelocity, targetY, currentAcceleration * deltaTime);
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
