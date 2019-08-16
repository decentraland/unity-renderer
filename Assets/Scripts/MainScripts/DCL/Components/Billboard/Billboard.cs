using System.Collections;
using DCL.Components;
using UnityEngine;

namespace DCL
{
    public class Billboard : BaseComponent
    {
        [System.Serializable]
        public class Model
        {
            public bool x = true;
            public bool y = true;
            public bool z = true;
        }

        public Model model = new Model();
        Transform entityTransform;

        public override IEnumerator ApplyChanges(string newJson)
        {
            DCLCharacterController.OnCharacterMoved += OnCharacterMoved;

            model = SceneController.i.SafeFromJson<Model>(newJson);

            yield return null;

            entityTransform = entity.gameObject.transform;
            ChangeOrientation();
        }

        Vector3 GetLookAtVector()
        {
            Vector3 lookAtDir = (DCLCharacterController.i.cameraTransform.position - entityTransform.position);

            // Note (Zak): This check is here to avoid normalizing twice if not needed
            if (!(model.x && model.y && model.z))
            {
                lookAtDir.Normalize();

                // Note (Zak): Model x,y,z are axis that we want to enable/disable
                // while lookAtDir x,y,z are the components of the look-at vector
                if (!model.x || model.z)
                    lookAtDir.y = entityTransform.forward.y;
                if (!model.y)
                    lookAtDir.x = entityTransform.forward.x;
            }

            return lookAtDir.normalized;
        }

        void OnCharacterMoved(DCLCharacterPosition position)
        {
            ChangeOrientation();
        }

        void ChangeOrientation()
        {
            if (entityTransform != null)
                entityTransform.forward = GetLookAtVector();
        }
    }
}
