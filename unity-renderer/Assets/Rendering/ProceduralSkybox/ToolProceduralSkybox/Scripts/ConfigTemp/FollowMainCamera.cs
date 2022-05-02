using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowMainCamera : MonoBehaviour
{
    public GameObject target;
    public bool followPos;
    public bool ignoreYPos;
    public bool followRot;

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        if (followPos)
        {
            Vector3 pos = target.transform.position;
            if (ignoreYPos)
            {
                pos.y = this.transform.position.y;
            }
            this.transform.position = pos;
        }

        if (followRot)
        {
            this.transform.rotation = target.transform.rotation;
        }
    }
}