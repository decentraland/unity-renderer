using UnityEngine;

namespace Builder
{
    public class DCLBuilderInput : MonoBehaviour
    {

        const string MouseXAxis = "Mouse X";
        const string MouseYAxis = "Mouse Y";
        const string MouseWheelAxis = "Mouse ScrollWheel";

        public delegate void MouseClickDelegate(int buttonId, Vector3 mousePosition);
        public delegate void MouseDragDelegate(int buttonId, Vector3 mousePosition, float axisX, float axisY);
        public delegate void MouseWheelDelegate(float axisValue);
        public delegate void KeyboardInputDelegate(KeyCode key);

        public static event MouseClickDelegate OnMouseDown;
        public static event MouseClickDelegate OnMouseUp;
        public static event MouseDragDelegate OnMouseDrag;
        public static event MouseWheelDelegate OnMouseWheel;
        public static event KeyboardInputDelegate OnKeyboardButtonHold;
        public static event KeyboardInputDelegate OnKeyboardButtonDown;
        public static event KeyboardInputDelegate OnKeyboardButtonUp;

        private float lastMouseWheelAxisValue = 0;

        //TODO: check this. most hotkeys should come from builder
        private KeyCode[] listenKeys = {
            KeyCode.UpArrow,
            KeyCode.DownArrow,
            KeyCode.LeftArrow,
            KeyCode.RightArrow,
            KeyCode.LeftShift,
            KeyCode.RightShift,
            KeyCode.Space,
            KeyCode.Minus,
            KeyCode.Underscore,
            KeyCode.Plus,
            KeyCode.Equals,
            KeyCode.W,
            KeyCode.E,
            KeyCode.I
        };

        private void Update()
        {
            for (int i = 0; i < 2; i++)
            {
                if (HasMouseButtonInput(i)) break;
            }
            UpdateMouseWheelInput();
            CheckKeyboardButtons();
        }

        private bool HasMouseButtonInput(int button)
        {
            if (Input.GetMouseButtonDown(button))
            {
                OnMouseDown?.Invoke(button, Input.mousePosition);
                return true;
            }
            else if (Input.GetMouseButton(button))
            {
                OnMouseDrag?.Invoke(button, Input.mousePosition, Input.GetAxis(MouseXAxis), Input.GetAxis(MouseYAxis));
                return true;
            }
            else if (Input.GetMouseButtonUp(button))
            {
                OnMouseUp?.Invoke(button, Input.mousePosition);
                return true;
            }

            return false;
        }

        private void UpdateMouseWheelInput()
        {
            float axisValue = Input.GetAxis(MouseWheelAxis);
            if (lastMouseWheelAxisValue != axisValue)
            {
                lastMouseWheelAxisValue = axisValue;
                OnMouseWheel?.Invoke(axisValue);
            }
        }

        private void CheckKeyboardButtons()
        {
            for (int i = 0; i < listenKeys.Length; i++)
            {
                if (Input.GetKeyDown(listenKeys[i]))
                {
                    OnKeyboardButtonDown?.Invoke(listenKeys[i]);
                }
                else if (Input.GetKey(listenKeys[i]))
                {
                    OnKeyboardButtonHold?.Invoke(listenKeys[i]);
                }
                else if (Input.GetKeyUp(listenKeys[i]))
                {
                    OnKeyboardButtonUp?.Invoke(listenKeys[i]);
                }
            }
        }
    }
}