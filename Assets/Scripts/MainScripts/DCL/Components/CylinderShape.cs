using System;
using System.Collections;
using System.Collections.Generic;
using DCL.Helpers;
using UnityEngine;

namespace DCL.Components
{
    public class CylinderShape : BaseShape
    {
        [System.Serializable]
        public class Model
        {
            public float radiusTop = 1f;        // Cone/Cylinder
            public float radiusBottom = 1f;     // Cone/Cylinder
            public float segmentsHeight = 1f;   // Cone/Cylinder
            public float segmentsRadial = 36f;  // Cone/Cylinder
            public bool openEnded = false;      // Cone/Cylinder
            public float? radius;               // Cone/Cylinder
            public float arc = 360f;            // Cone/Cylinder
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

            meshFilter.mesh = PrimitiveMeshBuilder.BuildCylinder(50, model.radiusTop, model.radiusBottom, 2f, 0f, true, false);

            ConfigureCollision(model.withCollisions);

            return null;
        }
    }
}
