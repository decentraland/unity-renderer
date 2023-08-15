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
        private Vector3 edgeSlipDelta;
        private Vector3 lastFinalVelocity; // this is the velocity that we want to move, it always has a value if you are pressing a key to move
        private Vector3 lastActualVelocity; // this is the actual velocity that moved the character, its 0 if you are moving against a wall
        private float lastImpactMagnitude; // this is 0 if you are at a wall running at it, it gets higher if you run at high velocities and impact the wall
        private int groundLayers;
        private Ray groundRay;

        private CameraMode.ModeId[] tpsCameraModes = { CameraMode.ModeId.ThirdPersonRight, CameraMode.ModeId.ThirdPersonLeft, CameraMode.ModeId.ThirdPersonCenter };
        private int currentCameraMode;

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

#region InputHandling
        public void Dispose()
        {
            jumpAction.OnStarted -= OnJumpPressed;
            jumpAction.OnFinished -= OnJumpReleased;
            sprintAction.OnStarted -= OnSprintPressed;
            sprintAction.OnFinished -= OnSprintReleased;
            walkAction.OnStarted -= OnWalkPressed;
            characterXAxis.OnValueChanged -= OnHorizontalInput;
            characterYAxis.OnValueChanged -= OnVerticalInput;
        }

        private void RegisterInputEvents()
        {
            jumpAction.OnStarted += OnJumpPressed;
            jumpAction.OnFinished += OnJumpReleased;
            sprintAction.OnStarted += OnSprintPressed;
            sprintAction.OnFinished += OnSprintReleased;
            walkAction.OnStarted += OnWalkPressed;
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

        private void OnJumpPressed(DCLAction_Hold active)
        {
            jumpTriggered = true;
            jumpButtonPressed = true;
        }

        private void OnJumpReleased(DCLAction_Hold active)
        {
            jumpTriggered = false;
            jumpButtonPressed = false;
        }

        private void OnSprintPressed(DCLAction_Hold active)
        {
            isRunning = true;
        }

        private void OnSprintReleased(DCLAction_Hold active)
        {
            isRunning = false;
        }

        private void OnWalkPressed(DCLAction_Hold active)
        {
            isWalking = !isWalking;
        }
#endregion

        public void Update(float deltaTime)
        {
            UpdateCheatKeys();

            // Update speed states only when grounded
            if (isGrounded)
                speedState = isRunning ? SpeedState.RUN : isWalking ? SpeedState.WALK : SpeedState.JOG;


            float targetVelocity = GetVelocityLimit();
            // linear lerp to the max velocity, we had this at first but I think we can safely remove this since it does not make sense.
            velocityLimit = Mathf.MoveTowards(velocityLimit, targetVelocity, Mathf.Max(velocityLimit, targetVelocity) * 2 * Time.deltaTime);

            CalculateHorizontalVelocity(deltaTime, velocityLimit);
            CalculateVerticalVelocity(deltaTime);

            ApplyDragToHorizontalVelocity();

            float finalVelocityMagnitude = GetFinalVelocityMagnitude(targetVelocity);

            // sum up all the velocities
            Vector3 finalVelocity = (Flat(externalImpulse) + Flat(velocity)).normalized * finalVelocityMagnitude;
            finalVelocity.y = externalImpulse.y + velocity.y;

            // external velocity is applied with no further mechanics
            finalVelocity += externalVelocity;

            Vector3 deltaPosition;
            bool currentGroundStatus;
            bool wallHit;
            Vector3 velocityDelta = finalVelocity * deltaTime;

            Vector3 downwardsSlopeModifier = GetSlopeVector(velocityDelta);

            // we finally move our character controller and we get some results
            (currentGroundStatus, deltaPosition, wallHit) = view.Move(velocityDelta + edgeSlipDelta + downwardsSlopeModifier);

            UpdateWallHitAnimation(deltaPosition, wallHit);

            // Update the latest grounded Y height to calculate the stun status once we are grounded again
            if (lastFinalVelocity.y >= 0 && finalVelocity.y < 0)
                lastUngroundPeakHeight = view.GetPosition().y;

            lastFinalVelocity = finalVelocity;

            // update the Edge Slip Mechanic
            Vector3 edgeModifier = GetSlopeModifier();
            edgeSlipDelta = edgeModifier * (data.edgeSlipSpeed * Time.deltaTime);

            if (!isGrounded && currentGroundStatus)
                OnJustGrounded();

            isGrounded = currentGroundStatus;

            if (isGrounded)
                lastGroundedTime = Time.time;

            UpdateExternalImpulse();

            UpdateShadowBlob();

            UpdateAnimationStates(deltaTime, finalVelocity, deltaPosition);
        }

        // update the state and flags for proper animations, we need a cleaner way of doing this
        private void UpdateAnimationStates(float deltaTime, Vector3 finalVelocity, Vector3 deltaPosition)
        {
            // long jump determines whether the jump animation is going to loop when going upwards
            characterState.IsLongJump = finalVelocity.y > (data.jogJumpHeight * 3 * data.jumpGravityFactor);
            // trigger the long fall animation
            characterState.IsLongFall = finalVelocity.y < -12;
            characterState.IsGrounded = isGrounded;
            // this triggers the upwards jump loop without jumping, this is mostly triggered when stepping on jump pads or vertical forces
            characterState.IsJumping = !isGrounded && (finalVelocity.y > 5f || characterState.IsLongJump);
            characterState.IsFalling = finalVelocity.y < -5f;
            characterState.TotalVelocity = deltaPosition / deltaTime;
            characterState.SpeedState = speedState;
            characterState.MaxVelocity = velocityLimit;
            characterState.IsStunned = isStunned;
        }

        private void UpdateWallHitAnimation(Vector3 deltaPosition, bool wallHit)
        {
            // we calculate the force of the impact to decide if we animate or not the wall hit
            Vector3 currentActualVelocity = lastFinalVelocity.normalized * (Flat(deltaPosition).magnitude / Time.deltaTime);
            float currentWallImpactMagnitude = (lastActualVelocity - currentActualVelocity).magnitude;
            if (wallHit && currentWallImpactMagnitude < 0.1f && lastImpactMagnitude > 1)
                characterState.WallHit();

            if (!wallHit)
                characterState.ResetWallHit();

            lastActualVelocity = currentActualVelocity;
            lastImpactMagnitude = currentWallImpactMagnitude;
        }

        private void OnJustGrounded()
        {
            // required for proper animation
            characterState.ResetWallHit();

            // to avoid accelerating too fast when just grounded
            accelerationWeight = 0;

            // We apply the stun mechanic if our last grounded position was high enough compared to our current position
            float deltaHeight = lastUngroundPeakHeight - view.GetPosition().y;
            if (deltaHeight > data.jumpHeightStun)
            {
                lastStunnedTime = Time.time;
                isStunned = true;
                isLongFall = false;
            }
        }

        // Debug tool to check where the character is going to land, we should remove this
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

        // Downwards slope mechanic, this allows us to stick to the ground when running towards downward slopes
        private Vector3 GetSlopeVector(Vector3 velocityDelta)
        {
            // disabled when jumping or not grounded
            if (!isGrounded || jumpButtonPressed) return Vector3.zero;

            Vector3 position = view.GetPosition();
            float feet = position.y - (characterControllerSettings.height * 0.5f);
            position.y = feet;

            groundRay.origin = position + velocityDelta;

            float downwardsSlopeDistance = speedState == SpeedState.RUN ? 0.55f : 0.45f;

            if (!Physics.Raycast(groundRay, out var hit, downwardsSlopeDistance, groundLayers))
                return Vector3.zero;

            float diff = feet - hit.point.y;

            return Vector3.down * diff;
        }

        // In order to avoid jumping faster than intended, we calculate the horizontal input velocity based on the current impulse
        private float GetFinalVelocityMagnitude(float targetVelocity)
        {
            float externalImpulseMagnitude = Flat(externalImpulse).magnitude;
            float sum = (Flat(externalImpulse) + Flat(velocity)).magnitude;

            // - is impulse magnitude
            // x is velocity magnitude
            // > or < is direction
            // | is targetVelocity

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

        // Edge slip mechanic
        // We utilize a downward spherecast to calculate in which part of our capsule we are hitting the ground, if its too far away from the threshold, we return that distance
        private Vector3 GetSlopeModifier()
        {
            if (isGrounded)
            {
                var settings = characterControllerSettings;

                Vector3 currentPosition = view.GetPosition();

                // spherecast downwards to check edges
                if (!Physics.SphereCast(currentPosition,
                        data.characterControllerRadius, Vector3.down, out sphereCastHitInfo,
                        1, groundLayers)) return Vector3.zero;

                Vector3 relativeHitPoint = sphereCastHitInfo.point - (currentPosition + settings.center);
                relativeHitPoint.y = 0;

                // raycast downwards to check if there's nothing, to avoid sliding on edges
                groundRay.origin = currentPosition;

                // to avoid sliding on slopes, we added an additional raycast check
                if (Physics.Raycast(groundRay, 1f, groundLayers))
                    return Vector3.zero;

                if (relativeHitPoint.magnitude > data.noSlipDistance)
                    return -relativeHitPoint;
            }

            return Vector3.zero;
        }

        private void ApplyDragToHorizontalVelocity()
        {
            // We apply horizontal drag to the velocity to de-accelerate during jumps, this made the jumping much nicer and the distances were shortened significantly
            if (isGrounded) return;

            var tempVelocity = Flat(velocity);
            tempVelocity = ApplyDrag(tempVelocity, data.airDrag * data.jumpVelocityDrag);
            tempVelocity.y = velocity.y;
            velocity = tempVelocity;
        }

        // These inputs are not coded correctly since they are not final
        private void UpdateCheatKeys()
        {
            // Apply an impulse towards the camera
            if (Input.GetKeyDown(KeyCode.Q))
            {
                Vector3 impulse = cameraForward.Get() * data.jumpPadForce;
                impulse.y += Mathf.Sqrt(-2 * GetJumpHeight(impulse) * (data.gravity * data.jumpGravityFactor));
                ApplyExternalImpulse(impulse);
            }

            // Apply an external velocity for debug purposes
            if (Input.GetKey(KeyCode.E))
                externalVelocity = Vector3.right * 5;
            else
                externalVelocity = Vector3.zero;

            // Change camera angle
            if (Input.GetKeyDown(KeyCode.J))
            {
                ChangeThirdPersonCameraMode();
            }

            // Change camera angle
            if (Input.GetKeyDown(KeyCode.K))
            {
                ChangeCameraToFeet();
            }
        }

        // Debug: This should not be here
        private void ChangeThirdPersonCameraMode()
        {
            currentCameraMode = (currentCameraMode + 1) % tpsCameraModes.Length;
            CameraMode.ModeId tpsCameraMode = tpsCameraModes[currentCameraMode];
            DataStore.i.Get<DataStore_Cursor>().cursorVisibleByCamera.Set(tpsCameraMode != CameraMode.ModeId.ThirdPersonCenter);
            cameraMode.Set(tpsCameraMode);
        }

        // Debug: This should not be here
        private void ChangeCameraToFeet()
        {
            cameraMode.Set(cameraMode.Get() == CameraMode.ModeId.Feet ? tpsCameraModes[currentCameraMode] : CameraMode.ModeId.Feet);
        }

        // We apply an additive impulse to the character that will be slowed down by drag forces
        // this impulse resets vertical velocity to reset the current gravity
        public void ApplyExternalImpulse(Vector3 impulse)
        {
            externalImpulse = impulse;
            view.SetForward(FlatNormal(externalImpulse));
            velocity.y = 0;
        }

        private void UpdateExternalImpulse()
        {
            if (isGrounded)
                externalImpulse.y = 0;

            externalImpulse = ApplyDrag(externalImpulse, isGrounded ? data.groundDrag : data.airDrag);

            // Drag gets inconsistent with low values so we implemented a threshold to completely freeze it at certain velocities
            if (externalImpulse.magnitude < data.minImpulse)
                externalImpulse = Vector3.zero;
        }

        // Drag force based on physics and simplified down by Chat GPT
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
                // trigger the animation explicitly
                characterState.Jump();

                // get the jump height based on the current velocity
                float jumpHeight = GetJumpHeight(Flat(lastActualVelocity));

                // upwards gravity is increased by a factor to fake faster jumps
                float upwardsGravity = data.gravity * data.jumpGravityFactor;

                // deterministic jump velocity equation to reach certain heights based on gravity
                float jumpStr = Mathf.Sqrt(-2 * jumpHeight * upwardsGravity);
                velocity.y += jumpStr;

                // flags and timers
                jumpTriggered = false;
                lastGroundedTime = 0; // to avoid double jumping
                lastJumpTime = Time.time;
            }

            // apply gravity factor if jump button is pressed for a long time (higher jump) [Not deterministic enough]
            if (jumpButtonPressed && Time.time - lastJumpTime < data.longJumpTime)
                targetGravity = data.gravity * data.longJumpGravityScale;

            // to fake faster jumps to avoid floaty low gravity behaviours, we increase the gravity only when going up
            if (velocity.y > 0)
                targetGravity *= data.jumpGravityFactor;

            // apply gravity
            if (!isGrounded)
                velocity.y += targetGravity * deltaTime;
        }

        private void CalculateHorizontalVelocity(float deltaTime, float velocityLimit)
        {
            // Long fall stun mechanic, if its triggered we skip the horizontal velocity calculation to avoid moving during the stun duration
            if (isStunned && Time.time - lastStunnedTime < data.longFallStunTime)
            {
                velocity.x = 0;
                velocity.z = 0;
                return;
            }

            isStunned = false;
            lastStunnedTime = 0;

            // Update axis velocity based on the input
            CalculateHorizontalInputVelocity(deltaTime, velocityLimit);

            // convert axis velocity to directional vectors using the camera angle
            var targetHorizontal = Vector3.zero;
            targetHorizontal += FlatNormal(cameraForward.Get()) * zVelocity;
            targetHorizontal += FlatNormal(cameraRight.Get()) * xVelocity;

            // velocity limit is based on current impulse, so we cannot move if impulse is too big
            targetHorizontal = Vector3.ClampMagnitude(targetHorizontal, Mathf.Max(velocityLimit - Flat(externalImpulse).magnitude, 0));

            // we rotate instantly our character controller, the view rotation is handled by the animation controller
            if (targetHorizontal.normalized.sqrMagnitude > 0.1f && (Mathf.Abs(xAxis) > 0 || Mathf.Abs(yAxis) > 0))
                view.SetForward(targetHorizontal.normalized);

            var horizontalVelocity = Flat(velocity);

            // air movement has its own acceleration
            if (!isGrounded)
                horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, targetHorizontal, data.airAcceleration * deltaTime);
            else
                horizontalVelocity = targetHorizontal;

            // apply the final velocity to the actual velocity
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
            // The desired velocity when running has an acceleration based on a curve (acceleration of acceleration)
            // so we lerp from min acceleration to max acceleration over time
            currentAcceleration = data.acceleration;

            int targetAccelerationWeight = Mathf.Abs(xAxis) > 0 || Mathf.Abs(yAxis) > 0 ? 1 : 0;
            accelerationWeight = Mathf.MoveTowards(accelerationWeight, targetAccelerationWeight, Time.deltaTime / data.accelerationTime);
            currentAcceleration = Mathf.Lerp(data.acceleration, data.maxAcceleration, data.accelerationCurve.Evaluate(accelerationWeight));

            // Before accelerating whe convert the sign of the current velocity to avoid de-accelerating when changing directions (AD-AD-ing), this improves the responsiveness by a lot
            // To de accelerate we use smooth damping
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
