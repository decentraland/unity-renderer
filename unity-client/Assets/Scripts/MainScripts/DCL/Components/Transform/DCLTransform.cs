using UnityEngine;

namespace DCL.Components
{
    public class DCLTransform
    {
        [System.Serializable]
        public class Model
        {
            public Vector3 position = Vector3.zero;
            public Quaternion rotation = Quaternion.identity;
            public Vector3 scale = Vector3.one;
        }

        public static Model model = new Model();
    }
}
