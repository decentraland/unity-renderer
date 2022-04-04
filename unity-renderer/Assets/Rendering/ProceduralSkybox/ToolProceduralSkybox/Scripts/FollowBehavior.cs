using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowBehavior : MonoBehaviour
{
    public GameObject targetCamera;
    public bool followRot;
    public bool followPos;

    private void LateUpdate()
    {
        if (targetCamera == null)
        {
            return;
        }
        if (followRot)
            this.transform.rotation = targetCamera.transform.rotation;
        if (followPos)
            this.transform.position = targetCamera.transform.position;
    }
}