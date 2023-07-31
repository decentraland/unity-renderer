using DCL;
using DCL.Helpers;
using DCL.Interface;
using JetBrains.Annotations;
using MainScripts.DCL.Controllers.CharacterController;
using System;
using System.Collections.Generic;
using UnityEngine;
using Environment = DCL.Environment;

namespace MainScripts.DCL.Controllers.CharacterControllerV2
{
    public class CharacterView : MonoBehaviour, ICharacterView
    {
        private const int FIELD_HEIGHT = 32;
        private const int LABEL_SIZE = 200;
        [SerializeField] private UnityEngine.CharacterController characterController;
        [SerializeField] private CharacterAnimationController animationController;

        [Header("Data")]
        [SerializeField] private CharacterControllerData data;

        [Header("InputActions")]
        [SerializeField] private InputAction_Hold jumpAction;
        [SerializeField] private InputAction_Hold sprintAction;
        [SerializeField] private InputAction_Hold walkAction;
        [SerializeField] private InputAction_Measurable characterYAxis;
        [SerializeField] private InputAction_Measurable characterXAxis;

        [SerializeField] private Vector3Variable cameraForward;
        [SerializeField] private Vector3Variable cameraRight;

        [Header("Old References")]
        public GameObject avatarGameObject;
        public GameObject firstPersonCameraGameObject;

        private DCLCharacterControllerV2 controller;
        private readonly DataStore_Player dataStorePlayer = DataStore.i.player;
        private bool initialPositionAlreadySet;
        private Vector3 lastPosition;

        // todo: delete this once the ui thing is done
        private Dictionary<string, string> valuesTemp = new ();
        private CharacterState characterState;

        // end delete

        private void Awake()
        {
            controller = new DCLCharacterControllerV2(this, data, jumpAction, sprintAction, walkAction, characterXAxis, characterYAxis, cameraForward, cameraRight);

            characterState = controller.GetCharacterState();
            animationController.SetupCharacterState(characterState);

            // TODO: Remove this?
            CommonScriptableObjects.playerUnityPosition.Set(Vector3.zero);
            dataStorePlayer.playerWorldPosition.Set(Vector3.zero);
            CommonScriptableObjects.playerCoords.Set(Vector2Int.zero);
            dataStorePlayer.playerGridPosition.Set(Vector2Int.zero);
            CommonScriptableObjects.playerUnityEulerAngles.Set(Vector3.zero);
            CommonScriptableObjects.worldOffset.OnChange += OnWorldReposition;
            var worldData = DataStore.i.Get<DataStore_World>();
            worldData.avatarTransform.Set(avatarGameObject.transform);
            worldData.fpsTransform.Set(firstPersonCameraGameObject.transform);
            CommonScriptableObjects.rendererState.OnChange += OnRenderingStateChanged;
            OnRenderingStateChanged(CommonScriptableObjects.rendererState.Get(), false);

            dataStorePlayer.lastTeleportPosition.OnChange += OnTeleport;
        }

        public void PostStart(FollowWithDamping cameraFollow)
        {
            cameraFollow.additionalHeight = characterController.height * 0.5f;
            animationController.PostStart(cameraFollow);
        }

        void OnRenderingStateChanged(bool isEnable, bool prevState)
        {
            this.enabled = isEnable && !DataStore.i.common.isSignUpFlow.Get();
        }

        [Obsolete]
        private void OnWorldReposition(Vector3 current, Vector3 previous)
        {
            transform.position = CharacterGlobals.characterPosition.unityPosition;
        }

        // sent by kernel
        [UsedImplicitly]
        public void Teleport(string teleportPayload)
        {
            var newPosition = Utils.FromJsonWithNulls<Vector3>(teleportPayload);
            dataStorePlayer.lastTeleportPosition.Set(newPosition, notifyEvent: true);

            if (!initialPositionAlreadySet) { initialPositionAlreadySet = true; }
        }

        private void OnTeleport(Vector3 current, Vector3 previous)
        {
            ReportPosition(current);
        }

        private void ReportPosition(Vector3 newPosition)
        {
            lastPosition = CharacterGlobals.characterPosition.worldPosition;
            CharacterGlobals.characterPosition.worldPosition = newPosition;
            transform.position = CharacterGlobals.characterPosition.unityPosition;
            Environment.i.platform.physicsSyncController?.MarkDirty();
            CommonScriptableObjects.playerUnityPosition.Set(CharacterGlobals.characterPosition.unityPosition);
            dataStorePlayer.playerWorldPosition.Set(CharacterGlobals.characterPosition.worldPosition);
            Vector2Int playerPosition = Utils.WorldToGridPosition(CharacterGlobals.characterPosition.worldPosition);
            CommonScriptableObjects.playerCoords.Set(playerPosition);
            dataStorePlayer.playerGridPosition.Set(playerPosition);
            dataStorePlayer.playerUnityPosition.Set(CharacterGlobals.characterPosition.unityPosition);

            if (Moved(lastPosition))
            {
                if (Moved(lastPosition, useThreshold: true))
                    ReportMovement();

                lastPosition = transform.position;
            }
        }

        bool Moved(Vector3 previousPosition, bool useThreshold = false)
        {
            if (useThreshold)
                return Vector3.Distance(CharacterGlobals.characterPosition.worldPosition, previousPosition) > 0.001f;
            else
                return CharacterGlobals.characterPosition.worldPosition != previousPosition;
        }

        void ReportMovement()
        {
            var reportPosition = CharacterGlobals.characterPosition.worldPosition;
            var compositeRotation = Quaternion.LookRotation(transform.forward);
            var cameraRotation = Quaternion.LookRotation(cameraForward.Get());

            if (initialPositionAlreadySet)
                WebInterface.ReportPosition(reportPosition, compositeRotation, characterController.height, cameraRotation);

            //lastMovementReportTime = DCLTime.realtimeSinceStartup;
        }

        private void Update()
        {
            controller.Update(Time.deltaTime);

            var tpsCamera = DataStore.i.camera.tpsCamera.Get();
            int targetFov = characterState.SpeedState == SpeedState.RUN && characterState.FlatVelocity.magnitude >= characterState.MaxVelocity * 0.35f ? 75 : 60;
            tpsCamera.m_Lens.FieldOfView = Mathf.MoveTowards(tpsCamera.m_Lens.FieldOfView, targetFov, 75 * Time.deltaTime);
        }

        public bool Move(Vector3 delta)
        {
            if (!initialPositionAlreadySet) return true;

            characterController.Move(delta);

            Vector3 transformPosition = transform.position;

            if (transformPosition.y < 0) { transform.position = new Vector3(transformPosition.x, 0, transformPosition.z); }

            ReportPosition(PositionUtils.UnityToWorldPosition(transformPosition));

            return characterController.isGrounded;
        }

        public void SetForward(Vector3 forward)
        {
            transform.forward = forward;
            CommonScriptableObjects.characterForward.Set(forward);
            CommonScriptableObjects.playerUnityEulerAngles.Set(transform.eulerAngles);
        }

        private void OnGUI()
        {
            var coef = Screen.width / 1920f; // values are made for 1920
            var firstColumnPosition = Mathf.RoundToInt(1920 * 0.12f);
            var secondColumnPosition = Mathf.RoundToInt(1920 * 0.6f);
            var fontSize = Mathf.RoundToInt(24 * coef);

            GUI.skin.label.fontSize = fontSize;
            GUI.skin.textField.fontSize = fontSize;
            var firstColumnYPos = 0;
            data.walkSpeed = DrawFloatField(firstColumnPosition, ref firstColumnYPos, data.walkSpeed, "walkSpeed");
            data.jogSpeed = DrawFloatField(firstColumnPosition, ref firstColumnYPos, data.jogSpeed, "jogSpeed");
            data.runSpeed = DrawFloatField(firstColumnPosition, ref firstColumnYPos, data.runSpeed, "runSpeed");
            data.acceleration = DrawFloatField(firstColumnPosition, ref firstColumnYPos, data.acceleration, "acceleration");
            data.airAcceleration = DrawFloatField(firstColumnPosition, ref firstColumnYPos, data.airAcceleration, "airAcceleration");
            data.gravity = DrawFloatField(firstColumnPosition, ref firstColumnYPos, data.gravity, "gravity");
            data.stopTimeSec = DrawFloatField(firstColumnPosition, ref firstColumnYPos, data.stopTimeSec, "stopTimeSec");
            data.walkJumpHeight = DrawFloatField(firstColumnPosition, ref firstColumnYPos, data.walkJumpHeight, "walkJumpHeight");
            data.jogJumpHeight = DrawFloatField(firstColumnPosition, ref firstColumnYPos, data.jogJumpHeight, "jogJumpHeight");
            data.runJumpHeight = DrawFloatField(firstColumnPosition, ref firstColumnYPos, data.runJumpHeight, "runJumpHeight");
            data.jumpGraceTime = DrawFloatField(firstColumnPosition, ref firstColumnYPos, data.jumpGraceTime, "jumpGraceTime");
            data.rotationSpeed = DrawFloatField(firstColumnPosition, ref firstColumnYPos, data.rotationSpeed, "rotationSpeed");
            data.jumpFakeTime = DrawFloatField(firstColumnPosition, ref firstColumnYPos, data.jumpFakeTime, "jumpFakeTime");
            data.jumpFakeCatchupSpeed = DrawFloatField(firstColumnPosition, ref firstColumnYPos, data.jumpFakeCatchupSpeed, "jumpFakeCatchupSpeed");

            var secondColumnYPos = 0;
            DrawObjectValue(secondColumnPosition, ref secondColumnYPos, "State", characterState.SpeedState);
            DrawObjectValue(secondColumnPosition, ref secondColumnYPos, "velocity", characterState.TotalVelocity);
            DrawObjectValue(secondColumnPosition, ref secondColumnYPos, "hSpeed", characterState.FlatVelocity.magnitude);
            DrawObjectValue(secondColumnPosition, ref secondColumnYPos, "ySpeed", characterState.TotalVelocity.y);
            DrawObjectValue(secondColumnPosition, ref secondColumnYPos, "isGrounded", characterState.IsGrounded);
            DrawObjectValue(secondColumnPosition, ref secondColumnYPos, "isFalling", characterState.IsJumping);
        }

        private void DrawObjectValue(int xPos, ref int yPos, string label, object obj)
        {
            DrawLabel(xPos, ref yPos, label);

            GUI.Label(
                new Rect(Width(xPos + FIELD_HEIGHT + LABEL_SIZE), Height(FIELD_HEIGHT + yPos),
                    Width(500), Height(FIELD_HEIGHT)), obj.ToString());

            yPos += FIELD_HEIGHT + 2;
        }

        private float DrawFloatField(int xPos, ref int yPos, float value, string label)
        {
            if (!valuesTemp.ContainsKey(label))
                valuesTemp.Add(label, value.ToString());

            DrawLabel(xPos, ref yPos, label);
            string result = GUI.TextField(new Rect(Width(xPos + FIELD_HEIGHT + LABEL_SIZE), Height(FIELD_HEIGHT + yPos), Width(90), Height(FIELD_HEIGHT)), valuesTemp[label]);
            valuesTemp[label] = result;
            yPos += FIELD_HEIGHT + 2;
            return float.TryParse(result, out float newNumber) ? newNumber : value;
        }

        private void DrawLabel(int xPos, ref int yPos, string label)
        {
            GUI.Label(new Rect(Width(xPos + FIELD_HEIGHT), Height(FIELD_HEIGHT + yPos), Width(LABEL_SIZE), Height(FIELD_HEIGHT)), label);
        }

        private float Width(float value) =>
            value * Screen.width / 1920f;

        private float Height(float value) =>
            value * Screen.height / 1080f;
    }

    public interface ICharacterView
    {
        bool Move(Vector3 delta);

        void SetForward(Vector3 forward);
    }
}
