using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Components
{

    public class DCLTransform : BaseComponent
    {

        [System.Serializable]
        public class Model
        {
            public Vector3 position;
            public Quaternion rotation;
            public Vector3 scale;
        }

        public override string componentName => "transform";
        public Model model = new Model();

        public override IEnumerator ApplyChanges(string newJson)
        {

            JsonUtility.FromJsonOverwrite(newJson, model);
            // this component is applied to the gameObjects transform
            if (gameObject != null)
            {
                var t = gameObject.transform;

                if (t.localPosition != model.position)
                {
                    t.localPosition = model.position;
                }

                if (t.localRotation != model.rotation)
                {
                    t.localRotation = model.rotation;
                }

                if (t.localScale != model.scale)
                {
                    t.localScale = model.scale;
                }
            }
            return null;
        }

        void OnDisable()
        {
            if (gameObject != null)
            {
                gameObject.transform.localPosition = Vector3.zero;
                gameObject.transform.localScale = Vector3.one;
                gameObject.transform.localRotation = Quaternion.identity;
            }
        }
    }
}
