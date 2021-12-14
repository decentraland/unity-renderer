using System.Collections;
using DCL.Configuration;
using DCL.Controllers;
using DCL.Models;
using UnityEngine;

namespace DCL.Components
{
    internal class AvatarAttachHandler
    {
        public AvatarAttachComponent.Model model { private set; get; } = new AvatarAttachComponent.Model();
        public IParcelScene scene { private set; get; }
        public IDCLEntity entity { private set; get; }

        private AvatarAttachComponent.Model prevModel = null;

        private IAvatarAnchorPoints anchorPoints;
        private AvatarAnchorPointIds anchorPointId;
        private Coroutine componentUpdate = null;

        private readonly AvatarAttachPlayerHandler avatarAttachPlayerHandler = new AvatarAttachPlayerHandler();
        private Vector3 scale = Vector3.one;

        public void Initialize(IParcelScene scene, IDCLEntity entity)
        {
            this.scene = scene;
            this.entity = entity;
            avatarAttachPlayerHandler.onAvatarDisconnect += Dettach;
        }

        public void OnModelUpdated(string json)
        {
            OnModelUpdated(model.GetDataFromJSON(json) as AvatarAttachComponent.Model);
        }

        public void OnModelUpdated(AvatarAttachComponent.Model newModel)
        {
            prevModel = model;
            model = newModel;

            if (model == null)
            {
                return;
            }

            Environment.i.world.sceneBoundsChecker?.AddEntityToBeChecked(entity);

            if (prevModel.avatarId != model.avatarId)
            {
                Dettach();

                if (!string.IsNullOrEmpty(model.avatarId))
                {
                    Attach(model.avatarId, (AvatarAnchorPointIds)model.anchorPointId);
                }
            }
        }

        public void CleanUp()
        {
            Dettach();
            avatarAttachPlayerHandler.onAvatarDisconnect -= Dettach;
            avatarAttachPlayerHandler.Dispose();
        }

        private void Dettach()
        {
            if (componentUpdate != null)
            {
                CoroutineStarter.Stop(componentUpdate);
                componentUpdate = null;
            }

            if (entity != null)
            {
                entity.gameObject.transform.localPosition = EnvironmentSettings.MORDOR;
            }

            avatarAttachPlayerHandler.CancelCurrentSearch();
        }

        private void Attach(string avatarId, AvatarAnchorPointIds anchorPointId)
        {
            avatarAttachPlayerHandler.SearchAnchorPoints(avatarId, anchorPoints =>
            {
                Attach(anchorPoints, anchorPointId);
            });
        }

        private void Attach(IAvatarAnchorPoints anchorPoints, AvatarAnchorPointIds anchorPointId)
        {
            this.anchorPoints = anchorPoints;
            this.anchorPointId = anchorPointId;

            if (componentUpdate == null)
            {
                componentUpdate = CoroutineStarter.Start(ComponentUpdate());
            }
        }

        IEnumerator ComponentUpdate()
        {
            while (true)
            {
                var anchorPoint = anchorPoints.GetTransfom(anchorPointId);

                entity.gameObject.transform.position = anchorPoint.position;
                entity.gameObject.transform.rotation = anchorPoint.rotation;

                yield return null;
            }
        }
    }
}