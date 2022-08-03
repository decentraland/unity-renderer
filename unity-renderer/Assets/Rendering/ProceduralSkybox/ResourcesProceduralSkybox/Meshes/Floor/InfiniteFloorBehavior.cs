using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfiniteFloorBehavior : MonoBehaviour
{
    public float offset;
    private void Start()
    {
        transform.position = new UnityEngine.Vector3(transform.position.x, -offset, transform.position.z);
    }

    void Update()
    {
        transform.position = new UnityEngine.Vector3(transform.position.x, -offset, transform.position.z);
    }
}
