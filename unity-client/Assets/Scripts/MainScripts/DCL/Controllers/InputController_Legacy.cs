using DCL.Configuration;
using DCL.Interface;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace DCL
{

    public class InputController_Legacy
    {
        private static bool renderingEnabled => CommonScriptableObjects.rendererState.Get();
        private static InputController_Legacy instance = null;

        public static InputController_Legacy i
        {
            get
            {
                if (instance == null)
                    instance = new InputController_Legacy();

                return instance;
            }
        }

        public enum EVENT
        {
            BUTTON_DOWN,
            BUTTON_UP
        }

        private enum BUTTON_TYPE
        {
            MOUSE,
            KEYBOARD
        }

        private struct BUTTON_MAP
        {
            public BUTTON_TYPE type;
            public int buttonNum;
            public WebInterface.ACTION_BUTTON buttonId;
            public bool useRaycast;
        }

        private Dictionary<WebInterface.ACTION_BUTTON, List<Action<WebInterface.ACTION_BUTTON, EVENT, bool>>> listeners = new Dictionary<WebInterface.ACTION_BUTTON, List<Action<WebInterface.ACTION_BUTTON, EVENT, bool>>>();
        private List<BUTTON_MAP> buttonsMap = new List<BUTTON_MAP>();

        private InputController_Legacy()
        {
            buttonsMap.Add(new BUTTON_MAP() { type = BUTTON_TYPE.MOUSE, buttonNum = 0, buttonId = WebInterface.ACTION_BUTTON.POINTER, useRaycast = true });
            buttonsMap.Add(new BUTTON_MAP() { type = BUTTON_TYPE.KEYBOARD, buttonNum = (int)InputSettings.PrimaryButtonKeyCode, buttonId = WebInterface.ACTION_BUTTON.PRIMARY, useRaycast = true });
            buttonsMap.Add(new BUTTON_MAP() { type = BUTTON_TYPE.KEYBOARD, buttonNum = (int)InputSettings.SecondaryButtonKeyCode, buttonId = WebInterface.ACTION_BUTTON.SECONDARY, useRaycast = true });
        }

        public void AddListener(WebInterface.ACTION_BUTTON buttonId, Action<WebInterface.ACTION_BUTTON, EVENT, bool> callback)
        {
            if (!listeners.ContainsKey(buttonId))
                listeners.Add(buttonId, new List<Action<WebInterface.ACTION_BUTTON, EVENT, bool>>());

            if (!listeners[buttonId].Contains(callback))
                listeners[buttonId].Add(callback);
        }

        public void RemoveListener(WebInterface.ACTION_BUTTON buttonId, Action<WebInterface.ACTION_BUTTON, EVENT, bool> callback)
        {
            if (listeners.ContainsKey(buttonId))
            {
                if (listeners[buttonId].Contains(callback))
                {
                    listeners[buttonId].Remove(callback);

                    if (listeners[buttonId].Count == 0)
                        listeners.Remove(buttonId);
                }
            }
        }

        // Note (Zak): it is public for testing purposes only
        public void RaiseEvent(WebInterface.ACTION_BUTTON buttonId, EVENT evt, bool useRaycast)
        {
            if (listeners.ContainsKey(buttonId))
            {
                List<Action<WebInterface.ACTION_BUTTON, EVENT, bool>> callbacks = listeners[buttonId];
                int count = callbacks.Count;

                for (int i = 0; i < count; i++)
                    callbacks[i].Invoke(buttonId, evt, useRaycast);
            }
        }

        public void Update()
        {
            if (!renderingEnabled)
                return;

            int count = buttonsMap.Count;

            for (int i = 0; i < count; i++)
            {
                BUTTON_MAP btnMap = buttonsMap[i];

                switch (btnMap.type)
                {
                    case BUTTON_TYPE.MOUSE:
                        if (CommonScriptableObjects.allUIHidden.Get()) break;
                        if (Input.GetMouseButtonDown(btnMap.buttonNum))
                            RaiseEvent(btnMap.buttonId, EVENT.BUTTON_DOWN, btnMap.useRaycast);
                        else if (Input.GetMouseButtonUp(btnMap.buttonNum))
                            RaiseEvent(btnMap.buttonId, EVENT.BUTTON_UP, btnMap.useRaycast);
                        break;
                    case BUTTON_TYPE.KEYBOARD:
                        if (CommonScriptableObjects.allUIHidden.Get()) break;
                        if (Input.GetKeyDown((KeyCode)btnMap.buttonNum))
                            RaiseEvent(btnMap.buttonId, EVENT.BUTTON_DOWN, btnMap.useRaycast);
                        else if (Input.GetKeyUp((KeyCode)btnMap.buttonNum))
                            RaiseEvent(btnMap.buttonId, EVENT.BUTTON_UP, btnMap.useRaycast);
                        break;
                }
            }
        }

        public bool IsPressed(WebInterface.ACTION_BUTTON button)
        {
            switch (button)
            {
                case WebInterface.ACTION_BUTTON.POINTER:
                    return Input.GetMouseButton(0);
                case WebInterface.ACTION_BUTTON.PRIMARY:
                    return Input.GetKey(InputSettings.PrimaryButtonKeyCode);
                case WebInterface.ACTION_BUTTON.SECONDARY:
                    return Input.GetKey(InputSettings.SecondaryButtonKeyCode);
                default: // ANY
                    return Input.GetMouseButton(0) ||
                            Input.GetKey(InputSettings.PrimaryButtonKeyCode) ||
                            Input.GetKey(InputSettings.SecondaryButtonKeyCode);
            }
        }
    }
}
