using System;
using Cinemachine.Utility;
using UnityEngine;

public class FollowWithDamping : MonoBehaviour
{
    public Transform target;
    public Vector3 damping;
    public float dampingChangeSpeed = 0.5f;

    [Header("Debug Zone")]
    public Vector3 currentDamping;

    private void Start()
    {
        currentDamping = damping;

        if (target != null)
        {
            transform.position = target.position;
            transform.forward = target.forward;
        }

        transform.parent = null;
    }

    private void OnEnable()
    {
        CommonScriptableObjects.worldOffset.OnChange += OnWorldOffsetChange;
    }

    private void OnDisable()
    {
        CommonScriptableObjects.worldOffset.OnChange -= OnWorldOffsetChange;
    }

    public void LateUpdate()
    {
        if ( target == null ) return;

        Vector3 myPosition = transform.position;
        Vector3 targetPosition = target.position;
        Vector3 finalPosition = myPosition;

        currentDamping += Damper.Damp( damping - currentDamping, Vector3.one * dampingChangeSpeed, Time.deltaTime);
        finalPosition += Damper.Damp(targetPosition - myPosition,  currentDamping, Time.deltaTime);

        Transform t = this.transform;
        t.position = finalPosition;
        t.forward = target.forward;
    }

    private void OnWorldOffsetChange(Vector3 current, Vector3 previous)
    {
        transform.position -= current - previous;
    }
}