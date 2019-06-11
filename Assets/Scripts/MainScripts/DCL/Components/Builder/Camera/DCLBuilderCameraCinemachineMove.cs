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
    Vector3 moveTargetAccumulated;
    Vector3 panIncrementTarget;
    Vector3 panAccumulated;
    Vector3 moveIncrement;
    Vector3 panIncrement;

    Vector3 panIncrementLast;


    Vector3 lastPanIncrement = Vector3.zero;

    Vector3 targetOriginalPosition;

    Transform target;
    public void Start()
    {
        targetOriginalPosition = Vector3.zero;
        panIncrementLast = Vector3.zero;
    }
    public void SetMoveAxis(Vector3 ma)
    {
        moveIncrement = ma;
    }

    public void SetPanAxis(Vector3 pa)
    {
        panIncrement = pa;
    }

    public void Reset()
    {
        moveTargetAccumulated = Vector3.zero;
        panIncrementTarget = Vector3.zero;
        panIncrementLast = Vector3.zero;

    }

    protected override void PostPipelineStageCallback(
       CinemachineVirtualCameraBase vcam,
       CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
    {
        if (targetOriginalPosition == Vector3.zero)
        {
            target = vcam.LookAt.GetComponent<CinemachineTargetGroup>().m_Targets[0].target;
            targetOriginalPosition = target.position;
        }

        var pos = state.RawPosition;

        //Pan movement
        Vector3 panInc = panIncrement - panIncrementLast;
        panIncrementTarget += vcam.transform.right * panInc.x;

        //Pan Movement Y
        state.RawPosition = pos + new Vector3(0, panIncrement.y, 0);

        //Move with arrow keys
        moveTargetAccumulated += vcam.transform.right * moveIncrement.x;

        Vector3 vCamForward = vcam.transform.forward;
        vCamForward.y = 0;
        vCamForward.Normalize();

        moveTargetAccumulated += vCamForward * moveIncrement.z;
        //Move target group
        target.position = targetOriginalPosition + moveTargetAccumulated + panIncrementTarget;

        moveIncrement = Vector3.zero;
        panIncrementLast = panIncrement;

    }
}
