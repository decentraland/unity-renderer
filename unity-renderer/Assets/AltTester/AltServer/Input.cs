#if ALTTESTER && ENABLE_LEGACY_INPUT_MANAGER

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Altom.AltDriver;
using Altom.AltTester;
using Altom.AltTester.InputModule;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Scripting;

[Preserve]
public class Input : MonoBehaviour
{
    private static bool _useCustomInput;
    private static UnityEngine.Vector3 _acceleration;
    private static UnityEngine.AccelerationEvent[] _accelerationEvents;
    private static int _touchCount;
    private static UnityEngine.Touch[] _touches = new UnityEngine.Touch[0];
    private static UnityEngine.Vector2 _mouseScrollDelta = new UnityEngine.Vector2();
    private static UnityEngine.Vector3 _mousePosition = new UnityEngine.Vector3();
    private static System.Collections.Generic.List<AltAxis> AxisList;
    private static GameObject eventSystemTargetMouseDown;
    private static GameObject monoBehaviourTargetMouseDown;
    private static UnityEngine.Vector3 previousMousePosition = new UnityEngine.Vector3();
    private static UnityEngine.GameObject monoBehaviourPreviousTarget = null;
    private static UnityEngine.GameObject previousEventSystemTarget = null;

    private static AltMockUpPointerInputModule _mockUpPointerInputModule;
    public static Input _instance;
    private static System.Collections.Generic.List<KeyStructure> _keyCodesPressed = new System.Collections.Generic.List<KeyStructure>();
    private static System.Collections.Generic.List<KeyStructure> _keyCodesPressedDown = new System.Collections.Generic.List<KeyStructure>();
    private static System.Collections.Generic.List<KeyStructure> _keyCodesPressedUp = new System.Collections.Generic.List<KeyStructure>();
    private static System.Collections.Generic.Dictionary<int, PointerEventData> _pointerEventsDataDictionary = new System.Collections.Generic.Dictionary<int, PointerEventData>();
    private static System.Collections.Generic.Dictionary<int, int> _inputIdDictionary = new System.Collections.Generic.Dictionary<int, int>();
    private static int mouseInputVisualiserId = -1;
    private static readonly KeyCode[] mouseKeyCodes = { KeyCode.Mouse0, KeyCode.Mouse1, KeyCode.Mouse2 };
    private static readonly Dictionary<PointerEventData.InputButton, int> pointerIds = new Dictionary<PointerEventData.InputButton, int>{{PointerEventData.InputButton.Left, -1},
                                                                                                                                {PointerEventData.InputButton.Right, -2},
                                                                                                                                {PointerEventData.InputButton.Middle, -3}};
    private static PointerEventData mouseDownPointerEventData = null;
    private static PointerEventData.InputButton[] mouseButtons = { PointerEventData.InputButton.Left, PointerEventData.InputButton.Middle, PointerEventData.InputButton.Right };

    public static float LastAxisValue { get; set; }
    public static string LastAxisName { get; set; }
    public static string LastButtonDown { get; set; }
    public static string LastButtonPressed { get; set; }
    public static string LastButtonUp { get; set; }

    public static bool UseCustomInput { get => _useCustomInput; set => _useCustomInput = value; }

    public static AltMockUpPointerInputModule AltMockUpPointerInputModule
    {
        get
        {
            if (_mockUpPointerInputModule == null)
            {
                if (EventSystem.current != null)
                {
                    _mockUpPointerInputModule = EventSystem.current.gameObject.AddComponent<AltMockUpPointerInputModule>();
                }
                else
                {
                    var newEventSystem = new GameObject("EventSystem");
                    _mockUpPointerInputModule = newEventSystem.AddComponent<AltMockUpPointerInputModule>();
                }
            }
            return _mockUpPointerInputModule;
        }
    }

    #region MonoBehaviour

    public void Start()
    {
        _instance = this;
        string filePath = "AltTester/AltTesterInputAxisData";

        UnityEngine.TextAsset targetFile = UnityEngine.Resources.Load<UnityEngine.TextAsset>(filePath);
        string dataAsJson = targetFile.text;
        AxisList = Newtonsoft.Json.JsonConvert.DeserializeObject<System.Collections.Generic.List<AltAxis>>(dataAsJson);
    }

    private void FixedUpdate()
    {
        var monoBehaviourTarget = FindObjectViaRayCast.GetGameObjectHitMonoBehaviour(mousePosition);
        if (monoBehaviourPreviousTarget != monoBehaviourTarget)
        {
            if (monoBehaviourPreviousTarget != null) monoBehaviourPreviousTarget.SendMessage("OnMouseExit", UnityEngine.SendMessageOptions.DontRequireReceiver);
            if (monoBehaviourTarget != null && previousMousePosition != mousePosition) monoBehaviourTarget.SendMessage("OnMouseEnter", UnityEngine.SendMessageOptions.DontRequireReceiver);
            monoBehaviourPreviousTarget = monoBehaviourTarget;
        }
        if (monoBehaviourTarget != null) monoBehaviourTarget.SendMessage("OnMouseOver", UnityEngine.SendMessageOptions.DontRequireReceiver);

        var pointerEventData = new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current)
        {
            position = mousePosition,
            button = UnityEngine.EventSystems.PointerEventData.InputButton.Left,
            eligibleForClick = true
        };
        var eventSystemTarget = findEventSystemObject(pointerEventData);
        pointerEventData.pointerEnter = eventSystemTarget;

        if (eventSystemTarget != previousEventSystemTarget)
        {
            if (previousEventSystemTarget != null) UnityEngine.EventSystems.ExecuteEvents.ExecuteHierarchy(previousEventSystemTarget, pointerEventData, UnityEngine.EventSystems.ExecuteEvents.pointerExitHandler);
            if (eventSystemTarget != null && previousMousePosition != mousePosition) UnityEngine.EventSystems.ExecuteEvents.ExecuteHierarchy(eventSystemTarget, pointerEventData, UnityEngine.EventSystems.ExecuteEvents.pointerEnterHandler);
            previousEventSystemTarget = eventSystemTarget;
        }
        if (previousMousePosition != mousePosition)
        {
            previousMousePosition = mousePosition;
        }

    }

    #endregion

    #region UnityEngine.Input.AltTester.NotImplemented

    public static bool simulateMouseWithTouches
    {
        get { return UnityEngine.Input.simulateMouseWithTouches; }
        set { UnityEngine.Input.simulateMouseWithTouches = value; }
    }

    public static bool mousePresent
    {
        get
        {
            return UnityEngine.Input.mousePresent;
        }
    }

    public static bool stylusTouchSupported
    {
        get { return UnityEngine.Input.stylusTouchSupported; }
    }

    public static bool touchSupported
    {
        get { return UnityEngine.Input.touchSupported; }
    }

    public static bool multiTouchEnabled
    {
        get { return UnityEngine.Input.multiTouchEnabled; }
        set { UnityEngine.Input.multiTouchEnabled = value; }
    }

    public static UnityEngine.LocationService location
    {
        get { return UnityEngine.Input.location; }
    }

    public static UnityEngine.Compass compass
    {
        get { return UnityEngine.Input.compass; }
    }

    public static UnityEngine.DeviceOrientation deviceOrientation
    {
        get { return UnityEngine.Input.deviceOrientation; }
    }

    public static UnityEngine.IMECompositionMode imeCompositionMode
    {
        get { return UnityEngine.Input.imeCompositionMode; }
        set { UnityEngine.Input.imeCompositionMode = value; }
    }

    public static string compositionString
    {
        get { return UnityEngine.Input.compositionString; }
    }

    public static bool imeIsSelected
    {
        get { return UnityEngine.Input.imeIsSelected; }
    }

    public static bool touchPressureSupported
    {
        get { return UnityEngine.Input.touchPressureSupported; }
    }

    public static UnityEngine.Gyroscope gyro
    {
        get { return UnityEngine.Input.gyro; }
    }

    public static UnityEngine.Vector2 compositionCursorPos
    {
        get { return UnityEngine.Input.compositionCursorPos; }
        set { UnityEngine.Input.compositionCursorPos = value; }
    }

    public static bool backButtonLeavesApp
    {
        get { return UnityEngine.Input.backButtonLeavesApp; }
        set { UnityEngine.Input.backButtonLeavesApp = value; }
    }

    [System.Obsolete]
    public static bool isGyroAvailable
    {
        get { return UnityEngine.Input.isGyroAvailable; }
    }

    public static bool compensateSensors
    {
        get { return UnityEngine.Input.compensateSensors; }
        set { UnityEngine.Input.compensateSensors = value; }
    }

    public static UnityEngine.AccelerationEvent GetAccelerationEvent(int index)
    {
        return UnityEngine.Input.GetAccelerationEvent(index);
    }

    public static string[] GetJoystickNames()
    {
        return UnityEngine.Input.GetJoystickNames();
    }

    public static void ResetInputAxes()
    {
        UnityEngine.Input.ResetInputAxes();
    }

    #endregion

    #region UnityEngine.Input.AltTester

    public static bool anyKey
    {
        get
        {
            if (_useCustomInput)
            {
                return _keyCodesPressed.Count > 0;
            }
            else
            {
                return UnityEngine.Input.anyKey;
            }
        }
    }

    public static bool anyKeyDown
    {
        get
        {
            if (_useCustomInput)
            {
                return _keyCodesPressedDown.Count > 0;
            }
            else
            {
                return UnityEngine.Input.anyKeyDown;
            }
        }
    }

    //WIP
    public static string inputString
    {
        get
        {
            if (_useCustomInput)
            {
                string charactersPressedCurrentFrame = "";
                foreach (var keyCode in _keyCodesPressedDown)
                {
                    //need a Parser from keycode to character every character from keyboard + backspace and enter
                }
                return charactersPressedCurrentFrame;

            }
            else
            {
                return UnityEngine.Input.inputString;
            }
        }
    }//TODO: Doable

    public static UnityEngine.Vector3 acceleration
    {
        get
        {
            if (_useCustomInput)
            {
                return _acceleration;
            }
            else
            {
                return UnityEngine.Input.acceleration;
            }
        }
        set
        {
            _acceleration = acceleration;
        }
    }

    public static UnityEngine.AccelerationEvent[] accelerationEvents
    {
        get
        {
            if (_useCustomInput)
            {
                return _accelerationEvents;
            }
            else
            {
                return UnityEngine.Input.accelerationEvents;
            }
        }
        set
        {
            _accelerationEvents = accelerationEvents;
        }
    }

    public static int accelerationEventCount
    {
        get
        {
            if (_useCustomInput)
            {
                return _accelerationEvents.Length;
            }
            else
            {
                return UnityEngine.Input.accelerationEventCount;
            }
        }
    }

    public static UnityEngine.Touch[] touches
    {
        get { return _useCustomInput ? _touches : UnityEngine.Input.touches; }
        set
        {
            _touches = value;
        }
    }

    public UnityEngine.Touch this[int i]
    {
        get { return _useCustomInput ? _touches[i] : UnityEngine.Input.GetTouch(i); }
        set { _touches[i] = value; }
    }

    public static int touchCount
    {
        get { return _useCustomInput ? _touchCount : UnityEngine.Input.touchCount; }
        set { _touchCount = value; }
    }

    public static UnityEngine.Vector2 mouseScrollDelta
    {
        get
        {
            if (_useCustomInput)
            {
                return _mouseScrollDelta;
            }
            else
            {
                return UnityEngine.Input.mouseScrollDelta;
            }
        }
    }

    public static UnityEngine.Vector3 mousePosition
    {
        get
        {
            if (_useCustomInput)
            {
                return _mousePosition;
            }
            else
            {
                return UnityEngine.Input.mousePosition;
            }
        }
        set
        {
            _mousePosition = value;
        }
    }//Doable

    public static float GetAxis(string axisName)
    {
        if (_useCustomInput)
        {
            var axis = AxisList.First(axle => axle.name == axisName);
            if (axis == null)
            {
                throw new NotFoundException("No axis with this name was found");
            }

            foreach (var keyStructure in _keyCodesPressed)
            {
                if ((axis.positiveButton != "" && keyStructure.KeyCode == ConvertStringToKeyCode(axis.positiveButton)) || (axis.altPositiveButton != "" && keyStructure.KeyCode == ConvertStringToKeyCode(axis.altPositiveButton)))
                {
                    LastAxisName = axisName;//DebugPurpose
                    LastAxisValue = keyStructure.Power;
                    return keyStructure.Power;
                }
                if ((axis.negativeButton != "" && keyStructure.KeyCode == ConvertStringToKeyCode(axis.negativeButton)) || (axis.altNegativeButton != "" && keyStructure.KeyCode == ConvertStringToKeyCode(axis.altNegativeButton)))
                {
                    LastAxisName = axisName;//DebugPurpose
                    LastAxisValue = -1 * keyStructure.Power;
                    return -1 * keyStructure.Power;
                }
            }
            return 0;
        }
        else
        {
            return UnityEngine.Input.GetAxis(axisName);
        }
    }

    public static float GetAxisRaw(string axisName)
    {
        if (_useCustomInput)
        {
            return GetAxis(axisName);
        }
        else
        {
            return UnityEngine.Input.GetAxisRaw(axisName);
        }
    }

    public static bool GetButton(string buttonName)
    {
        if (_useCustomInput)
        {
            var axis = AxisList.First(axle => axle.name == buttonName);

            if (axis == null)
            {
                throw new NotFoundException("No button with this name was found");
            }

            foreach (var keyStructure in _keyCodesPressed)
            {
                if (keyStructure.KeyCode == ConvertStringToKeyCode(axis.positiveButton) || keyStructure.KeyCode == ConvertStringToKeyCode(axis.altPositiveButton))
                {
                    LastAxisName = axis.name;//Debug purpose
                    return true;
                }
                if (keyStructure.KeyCode == ConvertStringToKeyCode(axis.negativeButton) || keyStructure.KeyCode == ConvertStringToKeyCode(axis.altNegativeButton))
                {
                    LastAxisName = axis.name;//Debug purpose
                    return true;
                }
            }
            return false;
        }
        else
        {
            return UnityEngine.Input.GetButton(buttonName);
        }
    }

    public static bool GetButtonDown(string buttonName)
    {

        if (_useCustomInput)
        {
            var axis = AxisList.First(axle => axle.name == buttonName);
            if (axis == null)
            {
                throw new NotFoundException("No button with this name was found");
            }
            foreach (var keyStructure in _keyCodesPressedDown)
            {
                if (keyStructure.KeyCode == ConvertStringToKeyCode(axis.positiveButton) || keyStructure.KeyCode == ConvertStringToKeyCode(axis.altPositiveButton))
                {
                    LastAxisName = axis.name;//Debug purpose
                    return true;
                }
                if (keyStructure.KeyCode == ConvertStringToKeyCode(axis.negativeButton) || keyStructure.KeyCode == ConvertStringToKeyCode(axis.altNegativeButton))
                {
                    LastAxisName = axis.name;//Debug purpose
                    return true;
                }
            }
            return false;
        }
        else
        {
            return UnityEngine.Input.GetButtonDown(buttonName);

        }
    }

    public static bool GetButtonUp(string buttonName)
    {

        if (_useCustomInput)
        {
            var axis = AxisList.First(axle => axle.name == buttonName);
            if (axis == null)
            {
                throw new NotFoundException("No button with this name was found");
            }
            foreach (var keyStructure in _keyCodesPressedUp)
            {
                if (keyStructure.KeyCode == ConvertStringToKeyCode(axis.positiveButton) || keyStructure.KeyCode == ConvertStringToKeyCode(axis.altPositiveButton))
                {
                    LastAxisName = axis.name;//Debug purpose
                    return true;
                }
                if (keyStructure.KeyCode == ConvertStringToKeyCode(axis.negativeButton) || keyStructure.KeyCode == ConvertStringToKeyCode(axis.altNegativeButton))
                {
                    LastAxisName = axis.name;//Debug purpose
                    return true;
                }
            }
            return false;
        }
        else
        {
            return UnityEngine.Input.GetButtonUp(buttonName);
        }
    }

    public static bool GetKey(string name)
    {
        if (_useCustomInput)
        {
            UnityEngine.KeyCode keyCode = (UnityEngine.KeyCode)System.Enum.Parse(typeof(UnityEngine.KeyCode), name);
            return 0 != _keyCodesPressed.FindAll(key => key.KeyCode == keyCode).Count;
        }
        else
        {
            return UnityEngine.Input.GetKey(name);
        }
    }

    public static bool GetKey(UnityEngine.KeyCode key)
    {
        if (_useCustomInput)
        {
            return 0 != _keyCodesPressed.FindAll(keyFromList => keyFromList.KeyCode == key).Count;
        }
        else
        {
            return UnityEngine.Input.GetKey(key);
        }
    }

    public static bool GetKeyDown(UnityEngine.KeyCode key)
    {
        if (_useCustomInput)
        {
            return 0 != _keyCodesPressedDown.FindAll(keyFromList => keyFromList.KeyCode == key).Count;
        }
        else
        {
            return UnityEngine.Input.GetKeyDown(key);
        }
    }

    public static bool GetKeyDown(string name)
    {

        if (_useCustomInput)
        {
            UnityEngine.KeyCode keyCode = (UnityEngine.KeyCode)System.Enum.Parse(typeof(UnityEngine.KeyCode), name);
            return 0 != _keyCodesPressedDown.FindAll(key => key.KeyCode == keyCode).Count;
        }
        else
        {
            return UnityEngine.Input.GetKeyDown(name);
        }
    }

    public static bool GetKeyUp(UnityEngine.KeyCode key)
    {
        if (_useCustomInput)
        {
            return 0 != _keyCodesPressedUp.FindAll(keyFromList => keyFromList.KeyCode == key).Count;
        }
        else
        {
            return UnityEngine.Input.GetKeyUp(key);
        }
    }

    public static bool GetKeyUp(string name)
    {
        if (_useCustomInput)
        {
            UnityEngine.KeyCode keyCode = (UnityEngine.KeyCode)System.Enum.Parse(typeof(UnityEngine.KeyCode), name);
            return 0 != _keyCodesPressedUp.FindAll(key => key.KeyCode == keyCode).Count;
        }
        else
        {
            return UnityEngine.Input.GetKeyUp(name);
        }
    }

    public static bool GetMouseButton(int button)
    {
        if (_useCustomInput)
        {
            var keyCode = (UnityEngine.KeyCode)System.Enum.Parse(typeof(UnityEngine.KeyCode), "Mouse" + button);
            return 0 != _keyCodesPressed.FindAll(key => key.KeyCode == keyCode).Count || touches.Length > button;
        }
        else
        {
            return UnityEngine.Input.GetMouseButton(button);
        }
    }

    public static bool GetMouseButtonDown(int button)
    {
        //method not tested
        if (_useCustomInput)
        {
            var keyCode = (UnityEngine.KeyCode)System.Enum.Parse(typeof(UnityEngine.KeyCode), "Mouse" + button);
            UnityEngine.Debug.Log("TEST:: " + (0 != _keyCodesPressedDown.FindAll(key => key.KeyCode == keyCode).Count || (touches.Length > button && touches[button].phase == UnityEngine.TouchPhase.Began)));
            return 0 != _keyCodesPressedDown.FindAll(key => key.KeyCode == keyCode).Count || (touches.Length > button && touches[button].phase == UnityEngine.TouchPhase.Began);
        }
        else
        {
            return UnityEngine.Input.GetMouseButtonDown(button);
        }
    }

    public static bool GetMouseButtonUp(int button)
    {
        //method not tested
        if (_useCustomInput)
        {
            var keyCode = (UnityEngine.KeyCode)System.Enum.Parse(typeof(UnityEngine.KeyCode), "Mouse" + button);
            return 0 != _keyCodesPressedUp.FindAll(key => key.KeyCode == keyCode).Count || touches.Length > button && touches[button].phase == UnityEngine.TouchPhase.Ended;
        }
        else
        {
            return UnityEngine.Input.GetMouseButtonUp(button);
        }
    }

    public static UnityEngine.Touch GetTouch(int index)
    {
        return _useCustomInput ? _touches[index] : UnityEngine.Input.GetTouch(index);
    }

    #endregion

    #region public commands interface
    public static int BeginTouch(UnityEngine.Vector3 screenPosition)
    {
        var touch = createTouch(screenPosition);

        if (touch.fingerId == 0)
            mousePosition = screenPosition;
        var pointerEventData = AltMockUpPointerInputModule.ExecuteTouchEvent(touch);
        _pointerEventsDataDictionary.Add(touch.fingerId, pointerEventData);
        _instance.StartCoroutine(setMouse0KeyCodePressedDown());
        var inputId = AltRunner._altRunner.ShowInput(touch.position);
        _inputIdDictionary.Add(touch.fingerId, inputId);

        return touch.fingerId;
    }

    public static void MoveTouch(int fingerId, Vector3 screenPosition)
    {
        var touch = findTouch(fingerId);
        var previousPointerEventData = _pointerEventsDataDictionary[touch.fingerId];
        var previousPosition = touch.position;
        touch.phase = TouchPhase.Moved;
        touch.position = screenPosition;
        touch.rawPosition = screenPosition;
        touch.deltaPosition = touch.position - previousPosition;
        if (fingerId == 0)
        {
            mousePosition = screenPosition;
        }
        updateTouchInTouchList(touch);
        AltMockUpPointerInputModule.ExecuteTouchEvent(touch, previousPointerEventData);
        var inputId = _inputIdDictionary[fingerId];
        AltRunner._altRunner.ShowInput(touch.position, inputId);

    }

    public static IEnumerator EndTouch(int fingerId)
    {
        yield return new WaitForEndOfFrame();

        var touch = findTouch(fingerId);
        var inputId = _inputIdDictionary[fingerId];
        _inputIdDictionary.Remove(touch.fingerId);
        AltRunner._altRunner.ShowInput(touch.position, inputId);
        var previousPointerEventData = _pointerEventsDataDictionary[touch.fingerId];
        _pointerEventsDataDictionary.Remove(touch.fingerId);

        var keyStructure = new KeyStructure(KeyCode.Mouse0, 1);
        beginKeyUpTouchEndedLifecycle(keyStructure, true, ref touch);
        AltMockUpPointerInputModule.ExecuteTouchEvent(touch, previousPointerEventData);

        yield return null;
        endKeyUpTouchEndedLifecycle(keyStructure, true, touch);
    }

    public static void SetMultipointSwipe(UnityEngine.Vector2[] positions, float duration, Action<Exception> onFinish)
    {
        _instance.StartCoroutine(runThrowingIterator(MultipointSwipeLifeCycle(positions, duration), onFinish));
    }

    public static System.Collections.IEnumerator MultipointSwipeLifeCycle(UnityEngine.Vector2[] positions, float duration)
    {
        var touch = new UnityEngine.Touch
        {
            phase = UnityEngine.TouchPhase.Began,
            position = positions[0]
        };

        System.Collections.Generic.List<UnityEngine.Touch> currentTouches = touches.ToList();
        currentTouches.Sort((touch1, touch2) => (touch1.fingerId.CompareTo(touch2.fingerId)));
        int fingerId = 0;
        foreach (var iter in currentTouches)
        {
            if (iter.fingerId != fingerId)
                break;
            fingerId++;
        }

        touch.fingerId = fingerId;
        touchCount++;

        var touchListCopy = new UnityEngine.Touch[touchCount];
        System.Array.Copy(touches, 0, touchListCopy, 0, touches.Length);
        touchListCopy[touchCount - 1] = touch;
        touches = touchListCopy;
        mousePosition = new UnityEngine.Vector3(touches[0].position.x, touches[0].position.y, 0);
        var pointerEventData = AltMockUpPointerInputModule.ExecuteTouchEvent(touch);
        var markId = AltRunner._altRunner.ShowInput(touch.position);

        yield return null;

        var oneInputDuration = duration / (positions.Length - 1);
        for (var i = 1; i < positions.Length; i++)
        {
            var wholeDelta = positions[i] - touch.position;
            var deltaPerSecond = wholeDelta / oneInputDuration;
            float time = 0;
            do
            {
                UnityEngine.Vector2 previousPosition = touch.position;
                if (time + UnityEngine.Time.unscaledDeltaTime < oneInputDuration)
                {
                    touch.position += deltaPerSecond * UnityEngine.Time.unscaledDeltaTime;
                }
                else
                {
                    touch.position = positions[i];
                }

                touch.phase = touch.deltaPosition != UnityEngine.Vector2.zero ? UnityEngine.TouchPhase.Moved : UnityEngine.TouchPhase.Stationary;
                time += UnityEngine.Time.unscaledDeltaTime;
                touch.deltaPosition = touch.position - previousPosition;
                updateTouchInTouchList(touch);
                mousePosition = new UnityEngine.Vector3(touches[0].position.x, touches[0].position.y, 0);
                pointerEventData = AltMockUpPointerInputModule.ExecuteTouchEvent(touch, pointerEventData);
                AltRunner._altRunner.ShowInput(touch.position, markId);
                yield return null;

            } while (time <= oneInputDuration);
        }

        yield return null;

        touch.phase = UnityEngine.TouchPhase.Ended;
        updateTouchInTouchList(touch);
        AltMockUpPointerInputModule.ExecuteTouchEvent(touch, pointerEventData);
        yield return null;
        var newTouches = new UnityEngine.Touch[touchCount - 1];
        int contor = 0;
        foreach (var t in touches)
        {
            if (t.fingerId != touch.fingerId)
            {
                newTouches[contor] = t;
                contor++;
            }
        }

        touches = newTouches;
        touchCount--;
    }

    public static void MoveMouse(UnityEngine.Vector2 location, float duration, Action<Exception> onFinish)
    {
        _instance.StartCoroutine(runThrowingIterator(MoveMouseCycle(location, duration), onFinish));
    }

    public static System.Collections.IEnumerator MoveMouseCycle(UnityEngine.Vector2 location, float duration)
    {
        float time = 0;
        var distance = location - new UnityEngine.Vector2(mousePosition.x, mousePosition.y);
        var inputId = mouseInputVisualiserId;
        if (mouseInputVisualiserId == -1)
        {
            inputId = AltRunner._altRunner.ShowInput(location);
            mouseInputVisualiserId = inputId;

        }
        else
        {
            AltRunner._altRunner.ShowInput(location, inputId);
        }
        do
        {
            UnityEngine.Vector3 delta;

            if (time + UnityEngine.Time.unscaledDeltaTime < duration)
            {
                delta = distance * UnityEngine.Time.unscaledDeltaTime / duration;
            }
            else
            {
                delta = location - new UnityEngine.Vector2(mousePosition.x, mousePosition.y);
            }

            mousePosition += delta;
            if (mouseDownPointerEventData != null)
            {
                _mockUpPointerInputModule.ExecuteDragPointerEvents(mouseDownPointerEventData);
                mouseDownPointerEventData.position = mousePosition;
                mouseDownPointerEventData.delta = delta;
                findEventSystemObject(mouseDownPointerEventData);
            }
            AltRunner._altRunner.ShowInput(mousePosition, inputId);
            yield return null;
            time += UnityEngine.Time.unscaledDeltaTime;
        } while (time < duration);
    }

    internal static System.Collections.IEnumerator ScrollLifeCycle(float scrollVertical, float scrollHorizontal, float duration)
    {
        float timeSpent = 0;

        while (timeSpent < duration)
        {
            yield return null;
            timeSpent += UnityEngine.Time.unscaledDeltaTime;
            float scrollVerticalStep = scrollVertical * UnityEngine.Time.unscaledDeltaTime / duration;
            float scrollHorizontalStep = scrollHorizontal * UnityEngine.Time.unscaledDeltaTime / duration;

            var pointerEventData = new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current)
            {
                position = _mousePosition,
                button = UnityEngine.EventSystems.PointerEventData.InputButton.Left,
                eligibleForClick = true,
            };
            var eventSystemTarget = findEventSystemObject(pointerEventData);
            _mouseScrollDelta = new UnityEngine.Vector2(scrollHorizontalStep, scrollVerticalStep);
            pointerEventData.scrollDelta = _mouseScrollDelta;
            UnityEngine.EventSystems.ExecuteEvents.ExecuteHierarchy(eventSystemTarget, pointerEventData, UnityEngine.EventSystems.ExecuteEvents.scrollHandler);
        }
        _mouseScrollDelta = UnityEngine.Vector2.zero;//reset the value after scroll ended
    }

    internal static IEnumerator runThrowingIterator( //TODO Remove this method when all the input methods were implemented in InputController
            IEnumerator enumerator,
            Action<Exception> done)
    {
        Exception err = null;
        while (true)
        {
            object current;
            try
            {
                if (enumerator.MoveNext() == false)
                {
                    break;
                }
                current = enumerator.Current;
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError(ex.ToString());
                err = ex;
                yield break;
            }
            yield return current;
        }
        done.Invoke(err);
    }
    #endregion

    #region private interface
    private static UnityEngine.Touch createTouch(UnityEngine.Vector3 screenPosition)
    {
        var touch = new UnityEngine.Touch
        {
            phase = UnityEngine.TouchPhase.Began,
            position = screenPosition,
            rawPosition = screenPosition,
            pressure = 1.0f,
            maximumPossiblePressure = 1.0f,
        };

        List<int> fingerIds = touches.Select(t => t.fingerId).ToList();
        fingerIds.Sort();
        int fingerId = 0;
        foreach (var iter in fingerIds)
        {
            if (iter != fingerId)
                break;
            fingerId++;
        }
        touch.fingerId = fingerId;


        touchCount++;
        var touchListCopy = new UnityEngine.Touch[touches.Length + 1];
        System.Array.Copy(touches, 0, touchListCopy, 0, touches.Length);
        touchListCopy[touches.Length] = touch;
        touches = touchListCopy;
        return touch;
    }

    private static void destroyTouch(UnityEngine.Touch touch)
    {
        var newTouches = new UnityEngine.Touch[touches.Length - 1];
        int contor = 0;
        foreach (var t in touches)
        {
            if (t.fingerId != touch.fingerId)
            {
                newTouches[contor] = t;
                contor++;
            }
        }

        touches = newTouches;
        touchCount--;
    }

    private static IEnumerator setMouse0KeyCodePressedDown()
    {
        var keyStructure = new KeyStructure(KeyCode.Mouse0, 1.0f);
        yield return new WaitForEndOfFrame();
        _keyCodesPressedDown.Add(keyStructure);
        _keyCodesPressed.Add(keyStructure);

        yield return null;
        _keyCodesPressedDown.Remove(keyStructure);
    }

    private static void beginKeyUpTouchEndedLifecycle(KeyStructure keyStructure, bool tap, ref Touch touch)
    {
        if (tap)
        {
            touch.phase = TouchPhase.Ended;
            updateTouchInTouchList(touch);
        }
        var pressedKeyStructure = _keyCodesPressed.Find(key => key.KeyCode == keyStructure.KeyCode);
        _keyCodesPressed.Remove(pressedKeyStructure);
        _keyCodesPressedUp.Add(keyStructure);
    }

    private static void endKeyUpTouchEndedLifecycle(KeyStructure keyStructure, bool tap, Touch touch)
    {
        _keyCodesPressedUp.Remove(keyStructure);
        if (tap)
        {
            destroyTouch(touch);
        }
    }

    private static Touch findTouch(int fingerId)
    {
        return touches.First(touch => touch.fingerId == fingerId);
    }

    /// <summary>
    /// Finds element at given pointerEventData for which we raise EventSystem input events
    /// </summary>
    /// <param name="pointerEventData"></param>
    /// <returns>the found gameObject</returns>
    private static UnityEngine.GameObject findEventSystemObject(UnityEngine.EventSystems.PointerEventData pointerEventData)
    {
        UnityEngine.EventSystems.RaycastResult firstRaycastResult;
        AltMockUpPointerInputModule.GetFirstRaycastResult(pointerEventData, out firstRaycastResult);
        pointerEventData.pointerCurrentRaycast = firstRaycastResult;
        pointerEventData.pointerPressRaycast = firstRaycastResult;
        return firstRaycastResult.gameObject;
    }

    internal static IEnumerator tapClickCoordinatesLifeCycle(UnityEngine.Vector2 screenPosition, int count, float interval, bool tap)
    {
        var pointerEventData = new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current)
        {
            position = screenPosition,
            button = UnityEngine.EventSystems.PointerEventData.InputButton.Left,
            eligibleForClick = true,
            pressPosition = screenPosition
        };
        var eventSystemTarget = findEventSystemObject(pointerEventData);
        var monoBehaviourTarget = FindObjectViaRayCast.FindMonoBehaviourObject(screenPosition);

        yield return new WaitForEndOfFrame();//run after Update

        mousePosition = screenPosition;
        pointerEventData.pointerEnter = eventSystemTarget;

        for (int i = 0; i < count; i++)
        {
            float time = 0;
            AltRunner._altRunner.ShowClick(screenPosition);

            /* pointer/touch down */
            UnityEngine.Touch touch = new UnityEngine.Touch();
            int pointerId = 0;
            if (tap)
            {
                touch = createTouch(screenPosition);
                pointerId = touch.fingerId;
            }
            pointerEventData.pointerId = pointerId;

            var keyStructure = new KeyStructure(UnityEngine.KeyCode.Mouse0, 1.0f);//power 1
            _keyCodesPressedDown.Add(keyStructure);
            _keyCodesPressed.Add(keyStructure);

            UnityEngine.EventSystems.ExecuteEvents.ExecuteHierarchy(eventSystemTarget, pointerEventData, UnityEngine.EventSystems.ExecuteEvents.initializePotentialDrag);

            pointerEventData.pointerPress = UnityEngine.EventSystems.ExecuteEvents.ExecuteHierarchy(eventSystemTarget, pointerEventData, UnityEngine.EventSystems.ExecuteEvents.pointerDownHandler);
            if (monoBehaviourTarget != null) monoBehaviourTarget.SendMessage("OnMouseDown", UnityEngine.SendMessageOptions.DontRequireReceiver);

            yield return null;
            time += UnityEngine.Time.unscaledDeltaTime;

            _keyCodesPressedDown.Remove(keyStructure);
            beginKeyUpTouchEndedLifecycle(keyStructure, tap, ref touch);

            UnityEngine.EventSystems.ExecuteEvents.ExecuteHierarchy(eventSystemTarget, pointerEventData, UnityEngine.EventSystems.ExecuteEvents.pointerUpHandler);
            if (monoBehaviourTarget != null) monoBehaviourTarget.SendMessage("OnMouseUp", UnityEngine.SendMessageOptions.DontRequireReceiver);

            UnityEngine.EventSystems.ExecuteEvents.ExecuteHierarchy(eventSystemTarget, pointerEventData, UnityEngine.EventSystems.ExecuteEvents.pointerClickHandler);
            if (monoBehaviourTarget != null) monoBehaviourTarget.SendMessage("OnMouseUpAsButton", UnityEngine.SendMessageOptions.DontRequireReceiver);

            yield return null;
            time += UnityEngine.Time.unscaledDeltaTime;

            endKeyUpTouchEndedLifecycle(keyStructure, tap, touch);

            if (i != count - 1 && time < interval)//do not wait at last click/tap
                yield return new UnityEngine.WaitForSecondsRealtime(interval - time);
        }

        // mouse position doesn't change  but we fire on mouse exit
        UnityEngine.EventSystems.ExecuteEvents.ExecuteHierarchy(eventSystemTarget, pointerEventData, UnityEngine.EventSystems.ExecuteEvents.pointerExitHandler);
        if (monoBehaviourTarget != null) monoBehaviourTarget.SendMessage("OnMouseExit", UnityEngine.SendMessageOptions.DontRequireReceiver);
    }

    internal static IEnumerator tapClickElementLifeCycle(UnityEngine.GameObject target, int count, float interval, bool tap)
    {
        UnityEngine.Vector3 screenPosition;
        AltRunner._altRunner.FindCameraThatSeesObject(target, out screenPosition);
        yield return new WaitForEndOfFrame();//run after Update

        var pointerEventData = new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current)
        {
            position = screenPosition,
            button = UnityEngine.EventSystems.PointerEventData.InputButton.Left,
            eligibleForClick = true,
            pressPosition = screenPosition
        };
        mousePosition = screenPosition;
        pointerEventData.pointerEnter = target;
        //repeat
        for (int i = 0; i < count; i++)
        {
            float time = 0;
            AltRunner._altRunner.ShowClick(screenPosition);

            /* pointer/touch down */
            UnityEngine.Touch touch = new UnityEngine.Touch();
            int pointerId = 0;
            if (tap)
            {
                touch = createTouch(screenPosition);
                pointerId = touch.fingerId;
            }
            pointerEventData.pointerId = pointerId;

            var keyStructure = new KeyStructure(UnityEngine.KeyCode.Mouse0, 1.0f);//power 1
            _keyCodesPressedDown.Add(keyStructure);
            _keyCodesPressed.Add(keyStructure);

            UnityEngine.EventSystems.ExecuteEvents.Execute(target, pointerEventData, UnityEngine.EventSystems.ExecuteEvents.initializePotentialDrag);

            UnityEngine.EventSystems.ExecuteEvents.Execute(target, pointerEventData, UnityEngine.EventSystems.ExecuteEvents.pointerDownHandler);
            if (target != null) target.SendMessage("OnMouseDown", UnityEngine.SendMessageOptions.DontRequireReceiver);
            pointerEventData.pointerPress = target;

            yield return null;
            time += UnityEngine.Time.unscaledDeltaTime;

            _keyCodesPressedDown.Remove(keyStructure);
            beginKeyUpTouchEndedLifecycle(keyStructure, tap, ref touch);


            UnityEngine.EventSystems.ExecuteEvents.Execute(target, pointerEventData, UnityEngine.EventSystems.ExecuteEvents.pointerUpHandler);
            if (target != null) target.SendMessage("OnMouseUp", UnityEngine.SendMessageOptions.DontRequireReceiver);

            UnityEngine.EventSystems.ExecuteEvents.Execute(target, pointerEventData, UnityEngine.EventSystems.ExecuteEvents.pointerClickHandler);
            if (target != null) target.SendMessage("OnMouseUpAsButton", UnityEngine.SendMessageOptions.DontRequireReceiver);

            yield return null;
            time += UnityEngine.Time.unscaledDeltaTime;

            endKeyUpTouchEndedLifecycle(keyStructure, tap, touch);

            if (i != count - 1 && time < interval)//do not wait at last click/tap
                yield return new UnityEngine.WaitForSecondsRealtime(interval - time);
        }

        // mouse position doesn't change  but we fire on mouse exit
        UnityEngine.EventSystems.ExecuteEvents.Execute(target, pointerEventData, UnityEngine.EventSystems.ExecuteEvents.pointerExitHandler);
        if (target != null) target.SendMessage("OnMouseExit", UnityEngine.SendMessageOptions.DontRequireReceiver);
    }

    private static void updateTouchInTouchList(Touch touch)
    {
        for (var t = 0; t < touches.Length; t++)
        {
            if (touches[t].fingerId == touch.fingerId)
            {
                touches[t] = touch;
            }
        }
    }

    internal static IEnumerator KeyDownLifeCycle(KeyCode keyCode, float power)
    {
        var keyStructure = new KeyStructure(keyCode, power);
        yield return null;
        _keyCodesPressedDown.Add(keyStructure);
        _keyCodesPressed.Add(keyStructure);
        yield return null;
        _keyCodesPressedDown.Remove(keyStructure);
        if (mouseKeyCodes.Contains(keyCode))
        {
            var inputButton = keyCodeToInputButton(keyCode);
            mouseTriggerInit(inputButton, out PointerEventData pointerEventData, out GameObject eventSystemTarget, out GameObject monoBehaviourTarget);
            mouseDownTrigger(inputButton, pointerEventData, eventSystemTarget, monoBehaviourTarget);
            mouseDownPointerEventData = pointerEventData;
        }
    }

    private static PointerEventData.InputButton keyCodeToInputButton(KeyCode keyCode)
    {
        PointerEventData.InputButton[] inputButtons = { PointerEventData.InputButton.Left, PointerEventData.InputButton.Right, PointerEventData.InputButton.Middle };
        return inputButtons[Array.IndexOf(mouseKeyCodes, keyCode)];
    }

    internal static IEnumerator KeyUpLifeCycle(KeyCode keyCode)
    {
        if (mouseKeyCodes.Contains(keyCode))
        {
            var inputButton = keyCodeToInputButton(keyCode);
            mouseTriggerInit(inputButton, out PointerEventData pointerEventData, out GameObject eventSystemTarget, out GameObject monoBehaviourTarget);
            mouseUpTrigger(inputButton, pointerEventData, eventSystemTarget, monoBehaviourTarget);
        }
        var keyStructure = new KeyStructure(keyCode, 1);
        _keyCodesPressed.Remove(keyStructure);
        _keyCodesPressedUp.Add(keyStructure);
        yield return null;
        _keyCodesPressedUp.Remove(keyStructure);
    }

    internal static IEnumerator KeyPressLifeCycle(KeyCode keyCode, float power, float duration)
    {
        var keyStructure = new KeyStructure(keyCode, power);
        yield return null;
        _keyCodesPressedDown.Add(keyStructure);
        _keyCodesPressed.Add(keyStructure);
        yield return null;
        _keyCodesPressedDown.Remove(keyStructure);
        if (mouseKeyCodes.Contains(keyCode))
        {
            var inputButton = keyCodeToInputButton(keyCode);
            yield return _instance.StartCoroutine(mouseEventTrigger(inputButton, duration));
        }
        else
        {
            if (duration != 0)
            {
                yield return new UnityEngine.WaitForSecondsRealtime(duration);
            }
        }
        _keyCodesPressed.Remove(keyStructure);
        _keyCodesPressedUp.Add(keyStructure);
        yield return null;
        _keyCodesPressedUp.Remove(keyStructure);
    }

    private static void mouseTriggerInit(PointerEventData.InputButton mouseButton, out PointerEventData pointerEventData, out GameObject eventSystemTarget, out GameObject monoBehaviourTarget)
    {
        pointerEventData = new PointerEventData(EventSystem.current)
        {
            position = mousePosition,
            button = mouseButton,
            eligibleForClick = true,
            pressPosition = mousePosition
        };
        eventSystemTarget = findEventSystemObject(pointerEventData);
        monoBehaviourTarget = FindObjectViaRayCast.FindMonoBehaviourObject(mousePosition);

    }

    private static void mouseDownTrigger(PointerEventData.InputButton mouseButton, PointerEventData pointerEventData, GameObject eventSystemTarget, GameObject monoBehaviourTarget)
    {

        AltRunner._altRunner.ShowClick(mousePosition);

        /* pointer/touch down */
        pointerEventData.pointerId = pointerIds[mouseButton];

        pointerEventData.pointerPress = ExecuteEvents.ExecuteHierarchy(eventSystemTarget, pointerEventData, UnityEngine.EventSystems.ExecuteEvents.pointerDownHandler);
        if (mouseButton == PointerEventData.InputButton.Left && monoBehaviourTarget != null) monoBehaviourTarget.SendMessage("OnMouseDown", UnityEngine.SendMessageOptions.DontRequireReceiver);

        if (mouseButtons.Contains(mouseButton))
        {
            pointerEventData.pointerDrag = ExecuteEvents.ExecuteHierarchy(eventSystemTarget, pointerEventData, ExecuteEvents.initializePotentialDrag);
            eventSystemTargetMouseDown = eventSystemTarget;
            monoBehaviourTargetMouseDown = monoBehaviourTarget;
        }
    }

    private static void mouseUpTrigger(PointerEventData.InputButton mouseButton, PointerEventData pointerEventData, GameObject eventSystemTarget, GameObject monoBehaviourTarget)
    {
        /* pointer/touch up */
        if (eventSystemTarget == eventSystemTargetMouseDown && mouseButton == PointerEventData.InputButton.Left)
        {
            ExecuteEvents.ExecuteHierarchy(eventSystemTarget, pointerEventData, ExecuteEvents.pointerClickHandler);
        }
        if (monoBehaviourTarget == monoBehaviourTargetMouseDown && mouseButton == PointerEventData.InputButton.Left && monoBehaviourTarget != null)
        {
            monoBehaviourTarget.SendMessage("OnMouseUpAsButton", SendMessageOptions.DontRequireReceiver);
        }

        ExecuteEvents.ExecuteHierarchy(eventSystemTarget, pointerEventData, ExecuteEvents.pointerUpHandler);
        if (mouseButton == PointerEventData.InputButton.Left && monoBehaviourTarget != null) monoBehaviourTarget.SendMessage("OnMouseUp", SendMessageOptions.DontRequireReceiver);

        if (mouseButtons.Contains(mouseButton) && mouseDownPointerEventData != null)
            _mockUpPointerInputModule.ExecuteEndDragPointerEvents(mouseDownPointerEventData);

        mouseDownPointerEventData = null;
    }

    private static IEnumerator mouseEventTrigger(PointerEventData.InputButton mouseButton, float duration)
    {
        mouseTriggerInit(mouseButton, out PointerEventData pointerEventData, out GameObject eventSystemTarget, out GameObject monoBehaviourTarget);
        mouseDownTrigger(mouseButton, pointerEventData, eventSystemTarget, monoBehaviourTarget);
        yield return new WaitForSecondsRealtime(duration);
        mouseUpTrigger(mouseButton, pointerEventData, eventSystemTarget, monoBehaviourTarget);
    }

    public static System.Collections.IEnumerator AccelerationLifeCycle(UnityEngine.Vector3 accelarationValue, float duration)
    {
        float timeSpent = 0;
        while (timeSpent < duration)
        {
            _acceleration = accelarationValue;
            yield return null;
            timeSpent += UnityEngine.Time.unscaledDeltaTime;
        }
        _acceleration = UnityEngine.Vector3.zero;//reset the value after acceleration ended
    }

    private static UnityEngine.KeyCode ConvertStringToKeyCode(string keyName)
    {
        if (keyName.Length == 1 && IsEnglishLetter(keyName[0]))
        {
            return (UnityEngine.KeyCode)System.Enum.Parse(typeof(UnityEngine.KeyCode), System.Char.ToUpper(keyName[0]).ToString());
        }
        if (keyName.Equals("left"))
        {
            return UnityEngine.KeyCode.LeftArrow;
        }
        if (keyName.Equals("right"))
        {
            return UnityEngine.KeyCode.RightArrow;
        }
        if (keyName.Equals("down"))
        {
            return UnityEngine.KeyCode.DownArrow;
        }
        if (keyName.Equals("up"))
        {
            return UnityEngine.KeyCode.UpArrow;
        }
        if (keyName.Length == 0 && char.IsDigit(keyName[0]))
        {
            return (UnityEngine.KeyCode)System.Enum.Parse(typeof(UnityEngine.KeyCode), "Alpha" + keyName);
        }
        if (System.Text.RegularExpressions.Regex.Match(keyName, @"\[[0-9]{1}\]").Success)
        {
            return (UnityEngine.KeyCode)System.Enum.Parse(typeof(UnityEngine.KeyCode), "Keypad" + keyName);
        }
        if (keyName == "[+]")
        {
            return UnityEngine.KeyCode.KeypadPlus;
        }
        if (keyName == "[equals]")
        {
            return UnityEngine.KeyCode.KeypadEquals;
        }
        if (System.Text.RegularExpressions.Regex.Match(keyName, "f[0-9]{1,2}").Success)
        {
            return (UnityEngine.KeyCode)System.Enum.Parse(typeof(UnityEngine.KeyCode), keyName.ToUpper());
        }
        if (keyName.Equals("right shift"))
        {
            return UnityEngine.KeyCode.RightShift;
        }
        if (keyName.Equals("left shift"))
        {
            return UnityEngine.KeyCode.LeftShift;
        }
        if (keyName.Equals("right ctrl"))
        {
            return UnityEngine.KeyCode.RightControl;
        }
        if (keyName.Equals("left ctrl"))
        {
            return UnityEngine.KeyCode.LeftControl;
        }
        if (keyName.Equals("right alt"))
        {
            return UnityEngine.KeyCode.RightAlt;
        }
        if (keyName.Equals("left alt"))
        {
            return UnityEngine.KeyCode.LeftAlt;
        }
        if (keyName.Equals("right cmd"))
        {
            return UnityEngine.KeyCode.RightCommand;
        }
        if (keyName.Equals("left cmd"))
        {
            return UnityEngine.KeyCode.LeftCommand;
        }
        if (System.Text.RegularExpressions.Regex.Match(keyName, @"mouse [0-6]").Success)
        {
            return (UnityEngine.KeyCode)System.Enum.Parse(typeof(UnityEngine.KeyCode), "Mouse" + keyName[6]);
        }
        if (keyName.Equals("backspace"))
        {
            return UnityEngine.KeyCode.Backspace;
        }
        if (keyName.Equals("tab"))
        {
            return UnityEngine.KeyCode.Tab;
        }
        if (keyName.Equals("return"))
        {
            return UnityEngine.KeyCode.Return;
        }
        if (keyName.Equals("escape"))
        {
            return UnityEngine.KeyCode.Escape;
        }
        if (keyName.Equals("space"))
        {
            return UnityEngine.KeyCode.Space;
        }
        if (keyName.Equals("delete"))
        {
            return UnityEngine.KeyCode.Delete;
        }
        if (keyName.Equals("enter"))
        {
            return UnityEngine.KeyCode.KeypadEnter;
        }
        if (keyName.Equals("insert"))
        {
            return UnityEngine.KeyCode.Insert;
        }
        if (keyName.Equals("home"))
        {
            return UnityEngine.KeyCode.Home;
        }
        if (keyName.Equals("end"))
        {
            return UnityEngine.KeyCode.End;
        }
        if (keyName.Equals("page up"))
        {
            return UnityEngine.KeyCode.PageUp;
        }
        if (keyName.Equals("page down"))
        {
            return UnityEngine.KeyCode.Home;
        }
        if (System.Text.RegularExpressions.Regex.Match(keyName, "joystick button [0-9]{1,2}").Success)
        {
            var splitedString = keyName.Split(' ');
            var number = System.Int32.Parse(splitedString[2]);
            if (number >= 20)
            {
                throw new NotFoundException("Key not recognized");
            }
            return (UnityEngine.KeyCode)System.Enum.Parse(typeof(UnityEngine.KeyCode), "JoystickButton" + number);
        }
        if (System.Text.RegularExpressions.Regex.Match(keyName, "joystick [1-8] button [0-9]{1,2}").Success)
        {
            var splitedString = keyName.Split(' ');
            var number = System.Int32.Parse(splitedString[3]);
            if (number >= 20)
            {
                throw new NotFoundException("Key not recognized");
            }
            return (UnityEngine.KeyCode)System.Enum.Parse(typeof(UnityEngine.KeyCode), "Joystick" + splitedString[1] + "Button" + number);
        }
        throw new NotFoundException("Key not recognized");
    }

    private static bool IsEnglishLetter(char c)
    {
        return (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z');
    }
    #endregion
}

namespace Altom.AltTester.InputModule
{

    public class KeyStructure
    {
        public KeyStructure(UnityEngine.KeyCode keyCode, float power)
        {
            KeyCode = keyCode;
            Power = power;
        }

        public UnityEngine.KeyCode KeyCode { get; set; }

        public float Power { get; set; }

        public override bool Equals(object obj)
        {
            if (!(obj is KeyStructure))
                return false;
            var other = (KeyStructure)obj;
            return
                other.KeyCode == this.KeyCode;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }
}
#else
using UnityEngine;

namespace Altom.AltTester.InputModule
{

    public class Input : MonoBehaviour
    {

    }
}
#endif
