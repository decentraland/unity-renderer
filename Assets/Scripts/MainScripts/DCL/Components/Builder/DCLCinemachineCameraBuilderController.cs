using UnityEngine;
using Cinemachine;
using System;
using UnityEngine.Experimental.UIElements;

[RequireComponent(typeof(CinemachineFreeLook))]
[RequireComponent(typeof(CinemachineCameraOffset))]
[RequireComponent(typeof(CinemachineCameraOffset))]

public class DCLCinemachineCameraBuilderController : MonoBehaviour
{
    public DCLBuilderCameraInput cameraInput;

    private CinemachineFreeLook freelook;
    private CinemachineCameraOffset cameraOffset;

    private DCLBuilderCameraCinemachineMove cameraMove;
    CinemachineFreeLook.Orbit[] originalOrbits = new CinemachineFreeLook.Orbit[0];

    [Header("Input axis management")]
    [AxisStateProperty]
    public AxisState orbitXAxis;

    [AxisStateProperty]
    public AxisState orbitYAxis;

    [AxisStateProperty]
    public AxisState panXAxis;

    [AxisStateProperty]
    public AxisState panYAxis;

    [AxisStateProperty]
    public AxisState zoomAxis;

    [Header("Initial Settings")]
    public float initialXAxis;

    public float initialYAxis;

    public float initialZoomAxis;

    [Header("Zoom Settings")]
    [Tooltip("The minimum scale for the orbits")]
    [Range(0.01f, 1f)]
    public float minScale = 0.5f;

    [Tooltip("The maximum scale for the orbits")]
    [Range(1F, 5f)]
    public float maxScale = 1;

    public float moveIncrement = 0.1f;

    // Use this for initialization
    void Start()
    {
        freelook = GetComponent<CinemachineFreeLook>();
        cameraOffset = GetComponent<CinemachineCameraOffset>();
        cameraMove = GetComponent<DCLBuilderCameraCinemachineMove>();
        Initialize();

        cameraInput.OnAxisInputChanged += UpdateAxisInput;
        cameraInput.OnButtonInputChanged += UpdateButtonInput;

        Reset();
    }

    private void Initialize()
    {
        // Special case with orbit X and orbit Y.
        // Make sure axisState configuration is the same between this class and freelook.
        freelook.m_XAxis = new AxisState(orbitXAxis.m_MinValue, orbitXAxis.m_MaxValue, orbitXAxis.m_Wrap, orbitXAxis.ValueRangeLocked, orbitXAxis.m_MaxSpeed, orbitXAxis.m_AccelTime, orbitXAxis.m_DecelTime, orbitXAxis.m_InputAxisName, orbitXAxis.m_InvertInput);
        freelook.m_YAxis = new AxisState(orbitYAxis.m_MinValue, orbitYAxis.m_MaxValue, orbitYAxis.m_Wrap, orbitYAxis.ValueRangeLocked, orbitYAxis.m_MaxSpeed, orbitYAxis.m_AccelTime, orbitYAxis.m_DecelTime, orbitYAxis.m_InputAxisName, orbitYAxis.m_InvertInput);

        // Set initial input
        UpdateAxisInput(DCLBuilderCameraInput.TAxis.ORBIT_X, cameraInput.GetAxis(DCLBuilderCameraInput.TAxis.ORBIT_X));
        UpdateAxisInput(DCLBuilderCameraInput.TAxis.ORBIT_Y, cameraInput.GetAxis(DCLBuilderCameraInput.TAxis.ORBIT_Y));
        UpdateAxisInput(DCLBuilderCameraInput.TAxis.PAN_X, cameraInput.GetAxis(DCLBuilderCameraInput.TAxis.PAN_X));
        UpdateAxisInput(DCLBuilderCameraInput.TAxis.PAN_Y, cameraInput.GetAxis(DCLBuilderCameraInput.TAxis.PAN_Y));
        UpdateAxisInput(DCLBuilderCameraInput.TAxis.ZOOM, cameraInput.GetAxis(DCLBuilderCameraInput.TAxis.ZOOM));

        UpdateButtonInput(DCLBuilderCameraInput.TButton.RESET, cameraInput.GetButton(DCLBuilderCameraInput.TButton.RESET));
        UpdateButtonInput(DCLBuilderCameraInput.TButton.ZOOM_IN, cameraInput.GetButton(DCLBuilderCameraInput.TButton.ZOOM_IN));
        UpdateButtonInput(DCLBuilderCameraInput.TButton.ZOOM_OUT, cameraInput.GetButton(DCLBuilderCameraInput.TButton.ZOOM_OUT));

        UpdateButtonInput(DCLBuilderCameraInput.TButton.MOVE_FORWARD, cameraInput.GetButton(DCLBuilderCameraInput.TButton.MOVE_FORWARD));
        UpdateButtonInput(DCLBuilderCameraInput.TButton.MOVE_BACKWARDS, cameraInput.GetButton(DCLBuilderCameraInput.TButton.MOVE_BACKWARDS));
        UpdateButtonInput(DCLBuilderCameraInput.TButton.MOVE_LEFT, cameraInput.GetButton(DCLBuilderCameraInput.TButton.MOVE_LEFT));
        UpdateButtonInput(DCLBuilderCameraInput.TButton.MOVE_RIGHT, cameraInput.GetButton(DCLBuilderCameraInput.TButton.MOVE_RIGHT));

    }

    private void UpdateAxisInput(DCLBuilderCameraInput.TAxis axis, float input)
    {
        switch (axis)
        {
            case DCLBuilderCameraInput.TAxis.ORBIT_X:
                orbitXAxis.m_InputAxisValue = input;
                break;
            case DCLBuilderCameraInput.TAxis.ORBIT_Y:
                orbitYAxis.m_InputAxisValue = input;
                break;
            case DCLBuilderCameraInput.TAxis.PAN_X:
                panXAxis.m_InputAxisValue = input;
                break;
            case DCLBuilderCameraInput.TAxis.PAN_Y:
                panYAxis.m_InputAxisValue = input;
                break;
            case DCLBuilderCameraInput.TAxis.ZOOM:
                zoomAxis.m_InputAxisValue = input;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(axis), axis, "Axis input is not handled.");
        }
    }

    private void UpdateButtonInput(DCLBuilderCameraInput.TButton button, bool input)
    {
        if (input)
        {
            switch (button)
            {
                case DCLBuilderCameraInput.TButton.RESET:
                    Reset();
                    break;
                case DCLBuilderCameraInput.TButton.ZOOM_IN:
                    ZoomIn();
                    break;
                case DCLBuilderCameraInput.TButton.ZOOM_OUT:
                    ZoomOut();
                    break;
                case DCLBuilderCameraInput.TButton.MOVE_FORWARD:
                    Move(new Vector3(0, 0, moveIncrement));
                    break;
                case DCLBuilderCameraInput.TButton.MOVE_BACKWARDS:
                    Move(new Vector3(0, 0, -moveIncrement));
                    break;
                case DCLBuilderCameraInput.TButton.MOVE_RIGHT:
                    Move(new Vector3(moveIncrement, 0, 0));
                    break;
                case DCLBuilderCameraInput.TButton.MOVE_LEFT:
                    Move(new Vector3(-moveIncrement, 0, 0));
                    break;
                default:
                    Debug.Log($"{button} state is {input}");
                    break;
            }
        }
    }

    void Update()
    {
        UpdateAxisStates(Time.deltaTime);

        UpdateOrbit();
        UpdatePan();
        UpdateZoom();
        UpdateMove();
    }

    private void UpdateOrbit()
    {
        freelook.m_XAxis.Value = orbitXAxis.Value;
        freelook.m_YAxis.Value = orbitYAxis.Value;
    }

    private void UpdateAxisStates(float deltaTime)
    {
        orbitXAxis.Update(deltaTime);
        orbitYAxis.Update(deltaTime);
        panXAxis.Update(deltaTime);
        panYAxis.Update(deltaTime);
        zoomAxis.Update(deltaTime);
    }


    void UpdatePan()
    {
        cameraOffset.m_Offset = new Vector3(panXAxis.Value, panYAxis.Value, 0);
    }

    void UpdateZoom()
    {
        if (originalOrbits.Length != freelook.m_Orbits.Length)
        {
            originalOrbits = new CinemachineFreeLook.Orbit[freelook.m_Orbits.Length];
            Array.Copy(freelook.m_Orbits, originalOrbits, freelook.m_Orbits.Length);
        }

        float scale = Mathf.Lerp(minScale, maxScale, zoomAxis.Value);
        for (int i = 0; i < Mathf.Min(originalOrbits.Length, freelook.m_Orbits.Length); i++)
        {
            freelook.m_Orbits[i].m_Height = originalOrbits[i].m_Height * scale;
            freelook.m_Orbits[i].m_Radius = originalOrbits[i].m_Radius * scale;
        }
    }

    void UpdateMove()
    {
        UpdateButtonInput(DCLBuilderCameraInput.TButton.MOVE_FORWARD, cameraInput.GetButton(DCLBuilderCameraInput.TButton.MOVE_FORWARD));
        UpdateButtonInput(DCLBuilderCameraInput.TButton.MOVE_BACKWARDS, cameraInput.GetButton(DCLBuilderCameraInput.TButton.MOVE_BACKWARDS));
        UpdateButtonInput(DCLBuilderCameraInput.TButton.MOVE_LEFT, cameraInput.GetButton(DCLBuilderCameraInput.TButton.MOVE_LEFT));
        UpdateButtonInput(DCLBuilderCameraInput.TButton.MOVE_RIGHT, cameraInput.GetButton(DCLBuilderCameraInput.TButton.MOVE_RIGHT));

    }

    public void Reset()
    {
        // TODO: remove inertia when resetting
        orbitXAxis.Value = initialXAxis;
        orbitYAxis.Value = initialYAxis;
        panXAxis.Value = 0f;
        panYAxis.Value = 0f;
        zoomAxis.Value = initialZoomAxis;
        cameraMove.Reset();
    }

    public void ZoomIn()
    {
        zoomAxis.Value -= 0.1f;
    }

    public void ZoomOut()
    {
        zoomAxis.Value += 0.1f;
    }

    void Move(Vector3 moveAxis)
    {
        cameraMove.SetMoveAxis(moveAxis);

    }
}
