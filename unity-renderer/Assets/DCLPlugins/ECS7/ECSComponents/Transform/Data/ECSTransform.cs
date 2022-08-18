using UnityEngine;

namespace DCL.ECSComponents
{
    public class ECSTransform
    {
        public Vector3 position = Vector3.zero;
        public Vector3 scale = Vector3.one;
        public Quaternion rotation = Quaternion.identity;
        public long parentId = 0;
    }
}