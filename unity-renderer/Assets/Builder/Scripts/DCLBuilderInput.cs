using UnityEngine;

namespace Builder
{
    public class DCLBuilderInput : MonoBehaviour
    {
        const string MouseXAxis = "Mouse X";
        const string MouseYAxis = "Mouse Y";

        [SerializeField] float mouseWheelThrottle = 0.1f;

        public delegate void MouseClickDelegate(int buttonId, Vector3 mousePosition);
        public delegate void MouseDragDelegate(int buttonId, Vector3 mousePosition, float axisX, float axisY);
        public delegate void MouseRawDragDelegate(int buttonId, Vector3 mousePosition, float axisX, float axisY);
        public delegate void MouseWheelDelegate(float axisValue);

        public static event MouseClickDelegate OnMouseDown;
        public static event MouseClickDelegate OnMouseUp;
        public static event MouseDragDelegate OnMouseDrag;
        public static event MouseRawDragDelegate OnMouseRawDrag;
        public static event MouseWheelDelegate OnMouseWheel;

        private float lastMouseWheelDelta = 0;
        private float lastMouseWheelTime = 0;

        private void Update()
        {
            
            for (int i = 0; i <= 2; i++)
            {
                if (HasMouseButtonInput(i)) break;
            }

            UpdateMouseWheelInput();

#if UNITY_EDITOR
            EditorKeyDownEvent();
#endif
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
                OnMouseRawDrag?.Invoke(button, Input.mousePosition, Input.GetAxisRaw(MouseXAxis), Input.GetAxisRaw(MouseYAxis));
                return true;
            }
            else if (Input.GetMouseButtonUp(button))
            {
                OnMouseUp?.Invoke(button, Input.mousePosition);
                return true;
            }

            return false;
        }

        private void OnMouseWheelInput(int delta)
        {
            if (lastMouseWheelDelta == delta)
            {
                if (Time.unscaledTime - lastMouseWheelTime >= mouseWheelThrottle)
                {
                    SetMouseWheelDelta(delta);
                }
            }
            else
            {
                SetMouseWheelDelta(delta);
            }
        }

        private void SetMouseWheelDelta(int delta)
        {
            OnMouseWheel?.Invoke(delta);
            lastMouseWheelTime = Time.unscaledTime;
            lastMouseWheelDelta = delta;
        }

        private void UpdateMouseWheelInput()
        {
            float axisValue = Input.GetAxis("Mouse ScrollWheel");
            if (axisValue != 0)
            {
                OnMouseWheelInput((int)Mathf.Sign(axisValue));
            }
        }

#if UNITY_EDITOR
        GameObject bridgeGameObject;

        private void EditorKeyDownEvent()
        {
            if (Input.GetKey(KeyCode.UpArrow))
            {
                SendMessageToBridge("OnBuilderKeyDown", "UpArrow");
            }
            if (Input.GetKey(KeyCode.DownArrow))
            {
                SendMessageToBridge("OnBuilderKeyDown", "DownArrow");
            }
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                SendMessageToBridge("OnBuilderKeyDown", "LeftArrow");
            }
            if (Input.GetKey(KeyCode.RightArrow))
            {
                SendMessageToBridge("OnBuilderKeyDown", "RightArrow");
            }
            if (Input.GetKey(KeyCode.LeftShift))
            {
                SendMessageToBridge("OnBuilderKeyDown", "LeftShift");
            }


            if (Input.GetKeyDown(KeyCode.W))
            {
                SendMessageToBridge("SelectGizmo", DCL.Components.DCLGizmos.Gizmo.MOVE);
            }
            if (Input.GetKeyDown(KeyCode.R))
            {
                SendMessageToBridge("SelectGizmo", DCL.Components.DCLGizmos.Gizmo.ROTATE);
            }
            if (Input.GetKeyDown(KeyCode.S))
            {
                SendMessageToBridge("SelectGizmo", DCL.Components.DCLGizmos.Gizmo.SCALE);
            }
        }

        private void SendMessageToBridge(string method, string arg)
        {
            if (bridgeGameObject == null)
            {
                bridgeGameObject = GameObject.Find("BuilderController");
            }
            if (bridgeGameObject != null)
            {
                bridgeGameObject.SendMessage(method, arg);
            }
        }
#endif

    }
}