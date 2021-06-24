using UnityEngine;

namespace DCL
{
    public class AvatarLODController : MonoBehaviour
    {
        // This runs on LateUpdate() instead of Update() to be applied AFTER the transform was moved by the transform component
        public void LateUpdate()
        {
            Vector3 previousForward = transform.forward;
            Vector3 lookAtDir = (transform.position - CommonScriptableObjects.cameraPosition).normalized;

            lookAtDir.y = previousForward.y;
            // lookAtDir.z = previousForward.z;

            transform.forward = lookAtDir;
        }
    }
}