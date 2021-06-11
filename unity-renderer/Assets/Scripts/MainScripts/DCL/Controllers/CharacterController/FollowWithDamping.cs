using System;
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
            transform.position = target.position;

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
        Vector3 myPosition = transform.position;
        Vector3 targetPosition = target.position;
        Vector3 finalPosition = myPosition;

        currentDamping += Damp( damping - currentDamping, Vector3.one * dampingChangeSpeed, Time.deltaTime);
        finalPosition += Damp(targetPosition - myPosition,  currentDamping, Time.deltaTime);

        Transform t = this.transform;
        t.position = finalPosition;
        t.forward = target.forward;
    }

    private void OnWorldOffsetChange(Vector3 current, Vector3 previous)
    {
        transform.position -= current - previous;
    }

    // NOTE: Code stolen from own cinemachine damping implementation
    const float Epsilon = 0.0001f;
    const float kLogNegligibleResidual = -4.605170186f;

    Vector3 Damp( Vector3 initial, Vector3 dampTime, float deltaTime )
    {
        Vector3 result = Vector3.zero;

        result.x += Damp(initial.x, dampTime.x, deltaTime);
        result.y += Damp(initial.y, dampTime.y, deltaTime);
        result.z += Damp(initial.z, dampTime.z, deltaTime);

        return result;
    }

    float Damp(float initial, float dampTime, float deltaTime)
    {
        if (dampTime < Epsilon || Mathf.Abs(initial) < Epsilon)
            return initial;
        if (deltaTime < Epsilon)
            return 0;
        float k = -kLogNegligibleResidual / dampTime;

        return initial * (1 - Mathf.Exp(-k * deltaTime));
    }
}