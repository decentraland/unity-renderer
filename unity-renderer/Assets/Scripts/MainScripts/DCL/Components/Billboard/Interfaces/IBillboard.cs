
using UnityEngine;

namespace DCL
{
    public interface IBillboard
    {
        public Transform Tr { get; set; }
        public Transform EntityTransform { get; set; }
        public Vector3 LastPosition { get; set; }


        public Vector3 GetLookAtVector(Vector3 cameraPosition);
    }
}