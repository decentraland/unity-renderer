using System.Collections;
using System.Collections.Generic;
using DCL.Controllers;
using UnityEngine;

public class BIWObjectSelector : MonoBehaviour
{
    public BIWRaycast builderRaycast;

    public static event System.Action<BIWGizmosAxis> OnGizmosAxisPressed;

    private void OnEnable() { BIWInputWrapper.OnMouseDown += OnMouseDown; }

    private void OnDisable() { BIWInputWrapper.OnMouseDown -= OnMouseDown; }

    private void OnMouseDown(int buttonId, Vector3 mousePosition)
    {
        if (buttonId != 0)
        {
            return;
        }

        RaycastHit hit;
        if (builderRaycast.Raycast(mousePosition, builderRaycast.gizmoMask, out hit, CompareSelectionHit))
        {
            BIWGizmosAxis gizmosAxis = hit.collider.gameObject.GetComponent<BIWGizmosAxis>();
            if (gizmosAxis != null)
            {
                OnGizmosAxisPressed?.Invoke(gizmosAxis);
            }
        }
    }

    private RaycastHit CompareSelectionHit(RaycastHit[] hits)
    {
        RaycastHit closestHit = hits[0];

        if (IsGizmoHit(closestHit)) // Gizmos has always priority
        {
            return closestHit;
        }
        return closestHit;
    }

    private bool IsGizmoHit(RaycastHit hit) { return hit.collider.gameObject.GetComponent<BIWGizmosAxis>() != null; }
}