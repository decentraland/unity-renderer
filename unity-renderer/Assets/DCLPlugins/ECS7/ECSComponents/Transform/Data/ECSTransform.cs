using UnityEngine;

namespace DCL.ECSComponents
{
    public class ECSTransform
    {
        public Vector3 position;
        public Vector3 scale;
        public Quaternion rotation;
        public long parentId;
    }
}