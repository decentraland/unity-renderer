using Cinemachine;
using DCL;
using DCL.Helpers;
using JetBrains.Annotations;
using MainScripts.DCL.Controllers.CharacterController;
using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;
using Environment = DCL.Environment;

namespace MainScripts.DCL.Controllers.CharacterControllerV2
{
    public class CharacterView : MonoBehaviour, ICharacterView
    {
        [SerializeField] private UnityEngine.CharacterController characterController;

        [Header("Data")]
        [SerializeField] private CharacterControllerData data;

        [Header("InputActions")]
        [SerializeField] private InputAction_Hold jumpAction;
        [SerializeField] private InputAction_Hold sprintAction;
        [SerializeField] private InputAction_Measurable characterYAxis;
        [SerializeField] private InputAction_Measurable characterXAxis;

        [SerializeField] private Vector3Variable cameraForward;
        [SerializeField] private Vector3Variable cameraRight;

        [Header("Old References")]
        public GameObject avatarGameObject;
        public GameObject firstPersonCameraGameObject;

        private DCLCharacterControllerV2 controller;
        private readonly DataStore_Player dataStorePlayer = DataStore.i.player;

        private void Awake()
        {
            controller = new DCLCharacterControllerV2(this, data, jumpAction, sprintAction, characterXAxis, characterYAxis, cameraForward, cameraRight);

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

            // REMOVE THIS AFTER WE ARE DONE EDITING VALUES
            var mouseCatcher = new GameObject("DebugMouseCatcher");
            var panel = new GameObject("Panel");
            panel.transform.SetParent(mouseCatcher.transform);
            mouseCatcher.AddComponent<RectTransform>();

            var background = panel.AddComponent<Image>();
            Color backgroundColor = Color.gray;
            backgroundColor.a = 0.15f;
            background.color = backgroundColor;

            var rectTransform = panel.GetComponent<RectTransform>();
            rectTransform.pivot = new Vector2(0, 1);
            rectTransform.anchorMin = new Vector2(0, 1);
            rectTransform.anchorMax = new Vector2(0, 1);
            rectTransform.anchoredPosition = new Vector2(300, -10f);
            rectTransform.sizeDelta = new Vector2(75+30+300, (7 * 40) + 14);

            var cv = mouseCatcher.AddComponent<Canvas>();
            cv.renderMode = RenderMode.ScreenSpaceOverlay;
            var rc = mouseCatcher.AddComponent<GraphicRaycaster>();
            rc.blockingMask = Physics.AllLayers;
        }

        [Obsolete]
        private void OnWorldReposition(Vector3 current, Vector3 previous)
        {
            Vector3 oldPos = this.transform.position;
            this.transform.position = CharacterGlobals.characterPosition.unityPosition;

            /*if (CinemachineCore.Instance.BrainCount > 0)
                CinemachineCore.Instance.GetActiveBrain(0).ActiveVirtualCamera?.OnTargetObjectWarped(transform, transform.position - oldPos);*/
        }

        // sent by kernel
        [UsedImplicitly]
        public void Teleport(string teleportPayload)
        {
            var newPosition = Utils.FromJsonWithNulls<Vector3>(teleportPayload);
            dataStorePlayer.lastTeleportPosition.Set(newPosition, notifyEvent: true);


            CharacterGlobals.characterPosition.worldPosition = newPosition;
            Environment.i.platform.physicsSyncController?.MarkDirty();
            CommonScriptableObjects.playerUnityPosition.Set(CharacterGlobals.characterPosition.unityPosition);
            dataStorePlayer.playerWorldPosition.Set(CharacterGlobals.characterPosition.worldPosition);
            Vector2Int playerPosition = Utils.WorldToGridPosition(CharacterGlobals.characterPosition.worldPosition);
            CommonScriptableObjects.playerCoords.Set(playerPosition);
            dataStorePlayer.playerGridPosition.Set(playerPosition);
            dataStorePlayer.playerUnityPosition.Set(CharacterGlobals.characterPosition.unityPosition);
        }

        private void Update()
        {
            controller.Update(Time.deltaTime);

        }

        public CollisionFlags Move(Vector3 delta)
        {
            CollisionFlags collisionFlags = characterController.Move(delta);

            Vector3 transformPosition = transform.position;

            if (transformPosition.y < 0)
            {
                transform.position = new Vector3(transformPosition.x, 0, transformPosition.z);
            }
            return collisionFlags;
        }

        public void SetForward(Vector3 forward)
        {
            transform.forward = forward;
            CommonScriptableObjects.characterForward.Set(forward);
            CommonScriptableObjects.playerUnityEulerAngles.Set(transform.eulerAngles);
        }

        private int accumulation = 0;
        private Dictionary<string, string> valuesTemp = new Dictionary<string, string>();

        private void OnGUI()
        {
            accumulation = 0;
            var baseWidth = 300;
            var separation1 = 200;

            GUI.skin.label.fontSize = 30;
            GUI.skin.textField.fontSize = 30;
            data.walkSpeed = DrawFloatSlider(baseWidth, separation1, data.walkSpeed, "walkSpeed");
            data.jogSpeed = DrawFloatSlider(baseWidth, separation1, data.jogSpeed, "jogSpeed");
            data.runSpeed = DrawFloatSlider(baseWidth, separation1, data.runSpeed, "runSpeed");
            data.acceleration = DrawFloatSlider(baseWidth, separation1, data.acceleration, "acceleration");
            data.airAcceleration = DrawFloatSlider(baseWidth, separation1, data.airAcceleration, "airAcceleration");
            data.gravity = DrawFloatSlider(baseWidth, separation1, data.gravity, "gravity");
            data.jumpHeight = DrawFloatSlider(baseWidth, separation1, data.jumpHeight, "jumpHeight");
        }


        private float DrawFloatSlider(int baseWidth, int separation1, float value, string label)
        {
            if (!valuesTemp.ContainsKey(label))
                valuesTemp.Add(label, value.ToString());

            var fieldHeight = 40;
            GUI.Label(new Rect(Width(baseWidth + fieldHeight), Height(fieldHeight + accumulation), Width(200), Height(fieldHeight)), label);
            string result = GUI.TextField(new Rect(Width(baseWidth + fieldHeight + separation1), Height(fieldHeight + accumulation), Width(100), Height(fieldHeight)), valuesTemp[label]);
            valuesTemp[label] = result;
            accumulation += fieldHeight + 2;
            return float.TryParse(result, out float newNumber) ? newNumber : value;
        }

        private float Width(float value) =>
            value * Screen.width / 1920f;

        private float Height(float value) =>
            value * Screen.height / 1080f;
    }

    public interface ICharacterView
    {
        CollisionFlags Move(Vector3 delta);

        void SetForward(Vector3 forward);
    }
}
