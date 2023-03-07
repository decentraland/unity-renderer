using System;
using DCL.Components;
using DCL.Configuration;
using DCL.Controllers;
using DCL.ECS7.InternalComponents;
using DCL.ECSRuntime;
using DCL.Models;
using UnityEngine;

namespace DCL.ECSComponents
{
    public class AvatarAttachComponentHandler : IECSComponentHandler<PBAvatarAttach>
    {
        internal PBAvatarAttach prevModel = null;

        internal IAvatarAnchorPoints anchorPoints;
        internal AvatarAnchorPointIds anchorPointId;
        private IDCLEntity entity;
        private IParcelScene scene;

        private Action componentUpdate = null;

        internal readonly GetAnchorPointsHandler getAnchorPointsHandler;
        private readonly IUpdateEventHandler updateEventHandler;
        private readonly IInternalECSComponent<InternalSceneBoundsCheck> sbcInternalComponent;

        private Vector2Int? currentCoords = null;
        private bool isInsideScene = true;

        public AvatarAttachComponentHandler(IUpdateEventHandler updateEventHandler, IInternalECSComponent<InternalSceneBoundsCheck> sbcInternalComponent)
        {
            this.updateEventHandler = updateEventHandler;
            this.sbcInternalComponent = sbcInternalComponent;

            getAnchorPointsHandler = new GetAnchorPointsHandler();
            getAnchorPointsHandler.OnAvatarRemoved += Detach;
        }

        public void OnComponentCreated(IParcelScene scene, IDCLEntity entity)
        {
            this.scene = scene;
            this.entity = entity;
        }

        public void OnComponentRemoved(IParcelScene scene, IDCLEntity entity)
        {
            Dispose();
        }

        public void OnComponentModelUpdated(IParcelScene scene, IDCLEntity entity, PBAvatarAttach model)
        {
            // If is the same model, we skip
            if (model == null || (prevModel != null && prevModel.AvatarId == model.AvatarId && model.AnchorPointId == prevModel.AnchorPointId))
                return;

            // Detach previous attachments
            Detach();
            Attach(model.AvatarId, (AvatarAnchorPointIds)model.AnchorPointId);

            prevModel = model;
        }

        public void Dispose()
        {
            Detach();
            getAnchorPointsHandler.OnAvatarRemoved -= Detach;
            getAnchorPointsHandler.Dispose();
        }

        internal virtual void Detach()
        {
            StopComponentUpdate();

            if (entity != null)
                entity.gameObject.transform.localPosition = EnvironmentSettings.MORDOR;

            getAnchorPointsHandler.CancelCurrentSearch();
        }

        internal virtual void Attach(string avatarId, AvatarAnchorPointIds anchorPointId)
        {
            getAnchorPointsHandler.SearchAnchorPoints(avatarId, anchorPoints =>
            {
                Attach(anchorPoints, anchorPointId);
            }, supportNullId: true);
        }

        internal virtual void Attach(IAvatarAnchorPoints anchorPoints, AvatarAnchorPointIds anchorPointId)
        {
            this.anchorPoints = anchorPoints;
            this.anchorPointId = anchorPointId;

            StartComponentUpdate();
        }

        internal void LateUpdate()
        {
            if (entity == null || scene == null)
            {
                StopComponentUpdate();
                return;
            }

            var anchorPoint = anchorPoints.GetTransform(anchorPointId);

            entity.gameObject.transform.position = anchorPoint.position;
            entity.gameObject.transform.rotation = anchorPoint.rotation;
            sbcInternalComponent.SetPosition(scene, entity, anchorPoint.position);
        }

        private void StartComponentUpdate()
        {
            if (componentUpdate != null)
                return;

            currentCoords = null;
            componentUpdate = LateUpdate;
            updateEventHandler?.AddListener(IUpdateEventHandler.EventType.LateUpdate, componentUpdate);
        }

        private void StopComponentUpdate()
        {
            if (componentUpdate == null)
                return;

            updateEventHandler?.RemoveListener(IUpdateEventHandler.EventType.LateUpdate, componentUpdate);
            componentUpdate = null;
        }
    }
}
