using System;
using UnityEngine;

namespace DCL.Components
{
    [Serializable]
    public abstract class DCLTransformModel : BaseModel
    {
        public Vector3 position = Vector3.zero;
        public Quaternion rotation = Quaternion.identity;
        public Vector3 scale = Vector3.one;
    }
}