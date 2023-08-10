using UnityEngine;

namespace MainScripts.DCL.Controllers.CharacterControllerV2
{
    public class RagdollController : MonoBehaviour
    {
        public Rigidbody[] bodies;
        public Collider[] colliders;

        [ContextMenu("Setup Components")]
        public void Setup()
        {
            bodies = GetComponentsInChildren<Rigidbody>();
            colliders = GetComponentsInChildren<Collider>();

            DeRagdollize();
        }

        public void Ragdollize()
        {
            foreach (Collider collider in colliders)
                collider.enabled = true;

            foreach (Rigidbody body in bodies)
                body.isKinematic = false;
        }

        public void DeRagdollize()
        {
            foreach (Rigidbody body in bodies)
            {
                body.isKinematic = true;
            }

            foreach (Collider collider in colliders)
            {
                collider.enabled = false;
            }
        }
    }
}
