using System.Collections;
using DCL.Configuration;
using DCL.Controllers;
using DCL.Models;
using UnityEngine;

namespace DCL.Components
{
    internal class AvatarAttachHandler
    {
        public AvatarAttachComponent.Model model { internal set; get; } = new AvatarAttachComponent.Model();
        public IParcelScene scene { private set; get; }
        public IDCLEntity entity { private set; get; }

        private AvatarAttachComponent.Model prevModel = null;

        private IAvatarAnchorPoints anchorPoints;
        private AvatarAnchorPointIds anchorPointId;

        private Coroutine componentUpdate = null;

        private readonly GetAnchorPointsHandler getAnchorPointsHandler = new GetAnchorPointsHandler();
        private ISceneBoundsChecker sceneBoundsChecker => Environment.i?.world?.sceneBoundsChecker;

        public void Initialize(IParcelScene scene, IDCLEntity entity)
        {
            this.scene = scene;
            this.entity = entity;
            getAnchorPointsHandler.OnAvatarRemoved += Detach;
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

            sceneBoundsChecker?.AddEntityToBeChecked(entity);

            if (prevModel.avatarId != model.avatarId)
            {
                Detach();

                if (!string.IsNullOrEmpty(model.avatarId))
                {
                    Attach(model.avatarId, (AvatarAnchorPointIds)model.anchorPointId);
                }
            }
        }

        public void CleanUp()
        {
            Detach();
            getAnchorPointsHandler.OnAvatarRemoved -= Detach;
            getAnchorPointsHandler.Dispose();
        }

        internal virtual void Detach()
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

            getAnchorPointsHandler.CancelCurrentSearch();
        }

        internal virtual void Attach(string avatarId, AvatarAnchorPointIds anchorPointId)
        {
            getAnchorPointsHandler.SearchAnchorPoints(avatarId, anchorPoints =>
            {
                Attach(anchorPoints, anchorPointId);
            });
        }

        internal virtual void Attach(IAvatarAnchorPoints anchorPoints, AvatarAnchorPointIds anchorPointId)
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
                if (entity == null)
                {
                    componentUpdate = null;
                    yield break;
                }

                var anchorPoint = anchorPoints.GetTransform(anchorPointId);

                entity.gameObject.transform.position = anchorPoint.position;
                entity.gameObject.transform.rotation = anchorPoint.rotation;

                yield return null;
            }
        }
    }
}