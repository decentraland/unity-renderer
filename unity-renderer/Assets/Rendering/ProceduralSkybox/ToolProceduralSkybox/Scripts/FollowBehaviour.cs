using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowBehaviour : MonoBehaviour
{
    public GameObject target;
    public bool followRot;
    public bool followPos;
    public bool ignoreYAxis = false;

    private void LateUpdate()
    {
        if (target == null)
            return;

        if (followRot)
            this.transform.rotation = target.transform.rotation;

        if (followPos)
        {
            Vector3 pos = target.transform.position;
            if (ignoreYAxis)
            {
                pos.y = 0;
            }
            this.transform.position = pos;
        }
    }
}