using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridPosition : MonoBehaviour
{
    public MeshRenderer floor;

    void Update()
    {
        floor.material.SetVector("_PlayerPosition", new Vector2(transform.position.x, transform.position.z));
    }
}
