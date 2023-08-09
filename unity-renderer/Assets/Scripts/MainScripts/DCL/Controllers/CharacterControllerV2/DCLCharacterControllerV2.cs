using DCL;
using DCL.CameraTool;
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
        private readonly CameraMode cameraMode;
        private readonly GameObject shadowBlob;

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
        private float lastUngroundPeakHeight;
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
        private Vector3 lastSlopeDelta;
        private Vector3 lastTargetVelocity; // this is the velocity that we want to move, it always has a value if you are pressing a key to move
        private Vector3 lastActualVelocity; // this is the actual velocity that moved the character, its 0 if you are moving against a wall
        private float lastImpactMagnitude; // this is 0 if you are at a wall running at it, it gets higher if you run at high velocities and impact the wall
        private int groundLayers;
        private Ray groundRay;

        private CameraMode.ModeId[] tpsCameraModes = new[] { CameraMode.ModeId.ThirdPersonRight, CameraMode.ModeId.ThirdPersonLeft, CameraMode.ModeId.ThirdPersonCenter };
        private int currentCameraMode = 0;

        public DCLCharacterControllerV2(ICharacterView view, CharacterControllerData data, IInputActionHold jumpAction, IInputActionHold sprintAction, InputAction_Hold walkAction,
            IInputActionMeasurable characterXAxis,
            IInputActionMeasurable characterYAxis, Vector3Variable cameraForward, Vector3Variable cameraRight, CameraMode cameraMode,
            GameObject shadowBlob)
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
            this.cameraMode = cameraMode;
            this.shadowBlob = shadowBlob;

            RegisterInputEvents();
            speedState = SpeedState.JOG;
            characterState = new CharacterState();
            velocityLimit = GetVelocityLimit();
            characterControllerSettings = view.GetCharacterControllerSettings();
            groundLayers = LayerMask.GetMask("Default", "Ground", "CharacterOnly");
            groundRay = new Ray(Vector3.zero, Vector3.down);
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

            float finalVelocityMagnitude = GetFinalVelocityMagnitude(targetVelocity);
            Vector3 finalVelocity = (Flat(externalImpulse) + Flat(velocity)).normalized * finalVelocityMagnitude;
            finalVelocity.y = externalImpulse.y + velocity.y;
            finalVelocity += externalVelocity;

            Vector3 deltaPosition;
            bool currentGroundStatus;
            bool wallHit;
            Vector3 velocityDelta = finalVelocity * deltaTime;

            Vector3 downwardsSlopeModifier = GetDownwardsSlopeBasedOnVelocity(velocityDelta);

            (currentGroundStatus, deltaPosition, wallHit) = view.Move(velocityDelta + lastSlopeDelta + downwardsSlopeModifier);

            Vector3 currentActualVelocity = lastTargetVelocity.normalized * (Flat(deltaPosition).magnitude / Time.deltaTime);

            var currentWallImpactMagnitude = (lastActualVelocity - currentActualVelocity).magnitude;
            if (wallHit && currentWallImpactMagnitude < 0.1f && lastImpactMagnitude > 1)
                characterState.WallHit();

            if (!wallHit)
                characterState.ResetWallHit();

            lastImpactMagnitude = currentWallImpactMagnitude;

            if (lastTargetVelocity.y >= 0 && finalVelocity.y < 0)
                lastUngroundPeakHeight = view.GetPosition().y;

            lastTargetVelocity = finalVelocity;
            lastActualVelocity = currentActualVelocity;

            Vector3 slope = GetSlopeModifier();
            lastSlopeDelta = slope * (data.slipSpeedMultiplier * Time.deltaTime);

            if (!isGrounded && currentGroundStatus)
                OnJustGrounded();

            isGrounded = currentGroundStatus;

            if (isGrounded)
                lastGroundedTime = Time.time;

            ApplyDragToImpulse();

            UpdateShadowBlob();

            // update the state for the animations
            characterState.IsLongJump = finalVelocity.y > (data.jogJumpHeight * 3 * data.jumpGravityFactor);
            characterState.IsLongFall = finalVelocity.y < -12;
            characterState.IsGrounded = isGrounded;
            characterState.IsJumping = !isGrounded && (finalVelocity.y > 5f || characterState.IsLongJump);
            characterState.IsFalling = finalVelocity.y < -5f;
            characterState.TotalVelocity = deltaPosition / deltaTime;
            characterState.SpeedState = speedState;
            characterState.MaxVelocity = velocityLimit;
            characterState.ExternalImpulse = externalImpulse;
            characterState.ExternalVelocity = externalVelocity;
            characterState.currentAcceleration = currentAcceleration;
            characterState.IsStunned = isStunned;
        }

        private void OnJustGrounded()
        {
            characterState.ResetWallHit();
            accelerationWeight = 0;

            float deltaHeight = lastUngroundPeakHeight - view.GetPosition().y;
            if (deltaHeight > data.jumpHeightStun)
            {
                lastStunnedTime = Time.time;
                isStunned = true;
                isLongFall = false;
            }
        }

        private void UpdateShadowBlob()
        {
            if (isGrounded)
            {
                shadowBlob.SetActive(false);
                return;
            }

            shadowBlob.SetActive(true);
            groundRay.origin = view.GetPosition();

            if (Physics.SphereCast(groundRay, data.characterControllerRadius , out var hit, 50f, groundLayers))
            {
                shadowBlob.transform.position = hit.point;
                shadowBlob.transform.up = hit.normal;
            }

        }

        private Vector3 GetDownwardsSlopeBasedOnVelocity(Vector3 velocityDelta)
        {
            if (!isGrounded || jumpButtonPressed) return Vector3.zero;

            Vector3 position = view.GetPosition();
            float feet = position.y - (characterControllerSettings.height * 0.5f);
            position.y = feet;

            groundRay.origin = position + velocityDelta;

            float downwardsSlopeDistance = speedState == SpeedState.RUN ? 0.55f : 0.45f;
            Debug.DrawLine(groundRay.origin, groundRay.origin + (groundRay.direction * downwardsSlopeDistance), Color.magenta, 0.15f);

            if (!Physics.Raycast(groundRay, out var hit, downwardsSlopeDistance, groundLayers))
                return Vector3.zero;

            float diff = feet - hit.point.y;

            return Vector3.down * diff;
        }

        // In order to avoid jumping faster than intended
        private float GetFinalVelocityMagnitude(float targetVelocity)
        {
            float externalImpulseMagnitude = Flat(externalImpulse).magnitude;
            float sum = (Flat(externalImpulse) + Flat(velocity)).magnitude;

            // ------<xx|> (velocity de-accelerates so magnitude is sum)
            if (sum < targetVelocity)
                return sum;

            // ---------|------<xxx (we allow deaccelerating)
            if (sum > targetVelocity && externalImpulseMagnitude < sum)
                return sum;

            // ---------|----> xxx> (magnitude is first)
            if (externalImpulseMagnitude > targetVelocity && sum > externalImpulseMagnitude)
                return externalImpulseMagnitude;

            // -----> xx|> (magnitude is limit)
            if (sum > targetVelocity)
                return targetVelocity;

            // --> xxx> |  (magnitude is sum)
            return sum;
        }

        private Vector3 GetSlopeModifier()
        {
            if (isGrounded)
            {
                var settings = characterControllerSettings;

                Vector3 currentPosition = view.GetPosition();

                // spherecast downwards to check slopes
                if (!Physics.SphereCast(currentPosition,
                        data.characterControllerRadius, Vector3.down, out sphereCastHitInfo,
                        1, groundLayers)) return Vector3.zero;

                Vector3 relativeHitPoint = sphereCastHitInfo.point - (currentPosition + settings.center);
                relativeHitPoint.y = 0;

                // raycast downwards to check if there's nothing, to avoid sliding on slopes
                groundRay.origin = currentPosition;

                if (Physics.Raycast(groundRay, 1f, groundLayers))
                    return Vector3.zero;

                if (relativeHitPoint.magnitude > data.noSlipDistance)
                    return -relativeHitPoint;
            }

            return Vector3.zero;
        }

        private void ApplyDragToVelocity()
        {
            if (!isGrounded)
            {
                var tempVelocity = Flat(velocity);
                tempVelocity = ApplyDrag(tempVelocity, data.airDrag * data.jumpVelocityDrag);
                tempVelocity.y = velocity.y;
                velocity = tempVelocity;
            }
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

            if (Input.GetKeyDown(KeyCode.J))
            {
                ChangeThirdPersonCameraMode();
            }

            if (Input.GetKeyDown(KeyCode.K))
            {
                ChangeCameraToFeet();
            }
        }

        private void ChangeCameraToFeet()
        {
            cameraMode.Set(cameraMode.Get() == CameraMode.ModeId.Feet ? tpsCameraModes[currentCameraMode] : CameraMode.ModeId.Feet);
        }

        private void ChangeThirdPersonCameraMode()
        {
            currentCameraMode = (currentCameraMode + 1) % tpsCameraModes.Length;
            CameraMode.ModeId tpsCameraMode = tpsCameraModes[currentCameraMode];
            DataStore.i.Get<DataStore_Cursor>().cursorVisibleByCamera.Set(tpsCameraMode != CameraMode.ModeId.ThirdPersonCenter);
            cameraMode.Set(tpsCameraMode);
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

            externalImpulse = ApplyDrag(externalImpulse, isGrounded ? data.groundDrag : data.airDrag);

            if (externalImpulse.magnitude < data.minImpulse)
                externalImpulse = Vector3.zero;
        }

        private Vector3 ApplyDrag(Vector3 vector, float drag)
        {
            float velocityMagnitude = vector.magnitude;
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
                float jumpHeight = GetJumpHeight(Flat(lastActualVelocity));
                float jumpStr = Mathf.Sqrt(-2 * jumpHeight * (data.gravity * data.jumpGravityFactor));
                velocity.y += jumpStr;
                /*var jumpImpulse = new Vector3(velocity.x, jumpStr, velocity.z);
                ApplyExternalImpulse(jumpImpulse);
                velocity.x = 0;
                velocity.z = 0;*/

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

        private float GetJumpHeight(Vector3 flatHorizontalVelocity)
        {
            float maxJumpHeight = speedState switch
                                      {
                                          SpeedState.WALK => data.jogJumpHeight,
                                          SpeedState.JOG => data.jogJumpHeight,
                                          SpeedState.RUN => data.runJumpHeight,
                                          _ => throw new ArgumentOutOfRangeException(),
                                      };

            float currentSpeed = flatHorizontalVelocity.magnitude;
            float jumpHeight = Mathf.Lerp(data.jogJumpHeight, maxJumpHeight, currentSpeed / data.runSpeed);
             return jumpHeight;
        }

        public CharacterState GetCharacterState() =>
            characterState;
    }
}
