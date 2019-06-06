using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

[ExecuteInEditMode]
[SaveDuringPlay]
[AddComponentMenu("")]
public class DCLBuilderCameraCinemachineMove : CinemachineExtension
{
    Vector3 moveAccumulated;
    Vector3 moveIncrement;
    public void SetMoveAxis(Vector3 ma)
    {
        moveIncrement = ma;
    }

    public void Reset()
    {
        moveAccumulated = Vector3.zero;
    }

    protected override void PostPipelineStageCallback(
       CinemachineVirtualCameraBase vcam,
       CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
    {
        var pos = state.RawPosition;
        moveAccumulated += vcam.transform.right * moveIncrement.x;
        Vector3 rawForward = vcam.transform.forward;
        rawForward.y = 0;
        rawForward.Normalize();
        moveAccumulated += rawForward * moveIncrement.z;
        state.RawPosition = pos + moveAccumulated;
        moveIncrement = Vector3.zero;

    }
}
