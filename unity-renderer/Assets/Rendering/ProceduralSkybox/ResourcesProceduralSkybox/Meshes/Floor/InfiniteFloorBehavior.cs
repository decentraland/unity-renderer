using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfiniteFloorBehavior : MonoBehaviour
{
    public float offset;
    void Start()
    {
        transform.position = new UnityEngine.Vector3(transform.position.x, transform.position.y - offset, transform.position.z);
    }
}
