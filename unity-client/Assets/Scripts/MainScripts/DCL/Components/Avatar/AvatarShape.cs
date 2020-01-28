using DCL.Components;
using System.Collections;
using UnityEngine;

namespace DCL
{
    public class AvatarShape : BaseComponent
    {
        public AvatarName avatarName;
        public AvatarRenderer avatarRenderer;
        public AvatarMovementController avatarMovementController;
        [SerializeField] internal GameObject minimapRepresentation;

        private string currentSerialization = "";
        public AvatarModel model = new AvatarModel();

        public bool everythingIsLoaded;

        void Start()
        {
            SetMinimapRepresentationActive(false);
        }

        void OnDestroy()
        {
            if (entity != null)
                entity.OnTransformChange = null;
        }


        public override IEnumerator ApplyChanges(string newJson)
        {
            //NOTE(Brian): Horrible fix to the double ApplyChanges call, as its breaking the needed logic.
            if (newJson == "{}")
                yield break;


            if (entity != null && entity.OnTransformChange == null)
            {
                entity.OnTransformChange += avatarMovementController.OnTransformChanged;
            }

            if (currentSerialization == newJson)
                yield break;

            model = SceneController.i.SafeFromJson<AvatarModel>(newJson);

            everythingIsLoaded = false;

            bool avatarDone = false;
            bool avatarFailed = false;

            yield return null; //NOTE(Brian): just in case we have a Object.Destroy waiting to be resolved.

            avatarRenderer.ApplyModel(model, () => avatarDone = true, () => avatarFailed = true);

            yield return new WaitUntil(() => avatarDone || avatarFailed);

            avatarName.SetName(model.name);
            SetMinimapRepresentationActive(true);
            everythingIsLoaded = true;
        }

        void SetMinimapRepresentationActive(bool active)
        {
            if (minimapRepresentation == null)
                return;

            minimapRepresentation.SetActive(active);
        }
    }
}
