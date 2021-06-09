using UnityEngine;

public class FollowWithDamping : MonoBehaviour
{
    public Transform target;
    public Vector3 damping;

    public void Start()
    {
        transform.parent = null;
    }

    public void LateUpdate()
    {
        Vector3 myPosition = transform.position;
        Vector3 targetPosition = target.position;
        Vector3 finalPosition = myPosition;

        finalPosition.x += Damp(targetPosition.x - myPosition.x,  damping.x, Time.deltaTime);
        finalPosition.y += Damp(targetPosition.y - myPosition.y, damping.y, Time.deltaTime);
        finalPosition.z += Damp(targetPosition.z - myPosition.z, damping.z, Time.deltaTime);

        transform.position = finalPosition;
        transform.forward = target.forward;
    }

    // NOTE: Code stolen from own cinemachine damping implementation
    const float Epsilon = 0.0001f;
    const float kLogNegligibleResidual = -4.605170186f;

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