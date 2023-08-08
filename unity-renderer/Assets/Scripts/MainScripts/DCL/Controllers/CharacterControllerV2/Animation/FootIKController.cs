using System;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class FootIKController : MonoBehaviour
{
    [SerializeField] private float raycastLenght;
    [SerializeField] private float weight;
    [SerializeField] private float weightSpeed;
    [SerializeField] private Rig feetRig;

    [Header("Feet IK")]
    [SerializeField] private TwoBoneIKConstraint leftFoot;
    [SerializeField] private TwoBoneIKConstraint rightFoot;

    [Header("Targets")]
    [SerializeField] private Transform leftFootTarget;
    [SerializeField] private Transform rightFootTarget;

    private LayerMask groundLayers;
    private Ray ray = new (Vector3.forward, Vector3.down);
    private RaycastHit hit;

    private void Awake()
    {
        groundLayers = LayerMask.GetMask("Default", "Ground", "CharacterOnly");
    }

    public void Update()
    {
        //feetRig.weight = weight;
        UpdateIk(leftFoot, leftFootTarget);
        UpdateIk(rightFoot, rightFootTarget);
    }

    private void UpdateIk(TwoBoneIKConstraint constraint, Transform target)
    {
        ray.origin = constraint.data.mid.position;

        var targetWeight = 0;
        Debug.DrawRay(ray.origin, ray.direction * raycastLenght, Color.red, 0.15f);
        if (Physics.Raycast(ray, out hit, raycastLenght, groundLayers))
        {
            targetWeight = 1;
            target.position = hit.point;
            target.up = -hit.normal;
        }

        //constraint.weight = Mathf.MoveTowards(constraint.weight, targetWeight, Time.deltaTime * weightSpeed);
    }
}
