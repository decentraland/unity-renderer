using DCL.Helpers;
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
            public Vector3 position = Vector3.zero;
            public Quaternion rotation = Quaternion.identity;
            public Vector3 scale = Vector3.one;
        }

        public override string componentName => "transform";
        public Model model = new Model();

        void UpdateTransform()
        {
            if (entity != null && entity.gameObject != null)
            {
                var t = entity.gameObject.transform;

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
        }


        public override IEnumerator ApplyChanges(string newJson)
        {
            model = Utils.SafeFromJson<Model>(newJson);
            UpdateTransform();
            return null;
        }

        void OnDisable()
        {
            if (entity != null && entity.gameObject != null)
            {
                entity.gameObject.transform.ResetLocalTRS();
            }
        }
    }
}
