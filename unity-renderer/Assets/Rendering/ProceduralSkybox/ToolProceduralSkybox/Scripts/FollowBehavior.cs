using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowBehavior : MonoBehaviour
{
    public GameObject target;
    public bool followRot;
    public bool followPos;

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }
        if (followRot)
            this.transform.rotation = target.transform.rotation;
        if (followPos)
            this.transform.position = target.transform.position;
    }
}