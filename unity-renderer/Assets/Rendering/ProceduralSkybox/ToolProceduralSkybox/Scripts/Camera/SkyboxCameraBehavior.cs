using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkyboxCameraBehavior : MonoBehaviour
{
    public GameObject targetCamera;

    private void LateUpdate() { this.transform.rotation = targetCamera.transform.rotation; }
}