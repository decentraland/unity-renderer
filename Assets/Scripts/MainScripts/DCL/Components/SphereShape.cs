using System;
using System.Collections;
using System.Collections.Generic;
using DCL.Helpers;
using UnityEngine;

namespace DCL.Components
{
    public class SphereShape : BaseShape
    {
        [System.Serializable]
        public class Model
        {
            public string tag;
            public bool withCollisions;
        }

        Model model = new Model();

        protected override void Awake()
        {
            base.Awake();

            if (meshFilter == null)
            {
                meshFilter = meshGameObject.AddComponent<MeshFilter>();
            }

            if (meshRenderer == null)
            {
                meshRenderer = meshGameObject.AddComponent<MeshRenderer>();
            }

            meshRenderer.sharedMaterial = Resources.Load<Material>("Materials/Default");
        }

        public override IEnumerator ApplyChanges(string newJson)
        {
            model = Helpers.Utils.SafeFromJson<Model>(newJson); // We don't use FromJsonOverwrite() to default the model properties on a partial json.

            meshFilter.mesh = PrimitiveMeshBuilder.BuildSphere(1f);

            ConfigureCollision(model.withCollisions);

            return null;
        }
    }
}
