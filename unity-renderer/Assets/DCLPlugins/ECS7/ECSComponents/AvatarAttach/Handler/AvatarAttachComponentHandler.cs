using System;
using DCL.Components;
using DCL.Configuration;
using DCL.Controllers;
using DCL.ECSRuntime;
using DCL.Models;
using UnityEngine;

namespace DCL.ECSComponents
{
    public class AvatarAttachComponentHandler : IECSComponentHandler<PBAvatarAttach>
    {
        private const float BOUNDARIES_CHECK_INTERVAL = 5;
        
        internal PBAvatarAttach prevModel = null;

        internal IAvatarAnchorPoints anchorPoints;
        internal AvatarAnchorPointIds anchorPointId;
        private IDCLEntity entity;
        private IParcelScene scene;

        private Action componentUpdate = null;

        internal readonly GetAnchorPointsHandler getAnchorPointsHandler;
        private readonly IUpdateEventHandler updateEventHandler;
        
        private Vector2Int? currentCoords = null;
        private bool isInsideScene = true;

        public AvatarAttachComponentHandler(IUpdateEventHandler updateEventHandler)
        {
            this.updateEventHandler = updateEventHandler;
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

            if (!string.IsNullOrEmpty(model.AvatarId))
            {
                // Attach the anchor point
                Attach(model.AvatarId, (AvatarAnchorPointIds)model.AnchorPointId);
            }

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
            });
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

            if (IsInsideScene(CommonScriptableObjects.worldOffset + anchorPoint.position))
            {
                entity.gameObject.transform.position = anchorPoint.position;
                entity.gameObject.transform.rotation = anchorPoint.rotation;
            }
            else
            {
                entity.gameObject.transform.localPosition = EnvironmentSettings.MORDOR;
            }
        }

        internal virtual bool IsInsideScene(UnityEngine.Vector3 position)
        {
            bool result = isInsideScene;

            Vector2Int coords = Helpers.Utils.WorldToGridPosition(position);
            
            if (currentCoords == null || currentCoords != coords)
                result = scene.IsInsideSceneBoundaries(coords, position.y);
            
            currentCoords = coords;
            isInsideScene = result;
            return result;
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