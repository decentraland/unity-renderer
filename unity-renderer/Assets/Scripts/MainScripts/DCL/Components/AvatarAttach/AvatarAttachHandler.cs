using System;
using DCL.Configuration;
using DCL.Controllers;
using DCL.Helpers;
using DCL.Models;
using UnityEngine;

namespace DCL.Components
{
    internal class AvatarAttachHandler : IDisposable
    {
        private const float BOUNDARIES_CHECK_INTERVAL = 5;

        public AvatarAttachComponent.Model model { internal set; get; } = new AvatarAttachComponent.Model();
        public IParcelScene scene { private set; get; }
        public IDCLEntity entity { private set; get; }

        private AvatarAttachComponent.Model prevModel = null;

        private IAvatarAnchorPoints anchorPoints;
        private AvatarAnchorPointIds anchorPointId;

        private Action componentUpdate = null;

        private readonly GetAnchorPointsHandler getAnchorPointsHandler = new GetAnchorPointsHandler();
        private ISceneBoundsChecker sceneBoundsChecker => Environment.i?.world?.sceneBoundsChecker;
        private IUpdateEventHandler updateEventHandler;

        private Vector2Int? currentCoords = null;
        private bool isInsideScene = true;
        private float lastBoundariesCheckTime = 0;

        public void Initialize(IParcelScene scene, IDCLEntity entity, IUpdateEventHandler updateEventHandler)
        {
            this.scene = scene;
            this.entity = entity;
            this.updateEventHandler = updateEventHandler;
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

            if (prevModel.avatarId != model.avatarId)
            {
                Detach();

                if (!string.IsNullOrEmpty(model.avatarId))
                {
                    Attach(model.avatarId, (AvatarAnchorPointIds)model.anchorPointId);
                }
            }
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

                if (Time.unscaledTime - lastBoundariesCheckTime > BOUNDARIES_CHECK_INTERVAL)
                {
                    CheckSceneBoundaries(entity);
                }
            }
            else
            {
                entity.gameObject.transform.localPosition = EnvironmentSettings.MORDOR;
            }
        }

        internal virtual bool IsInsideScene(Vector3 position)
        {
            bool result = isInsideScene;
            Vector2Int coords = Utils.WorldToGridPosition(position);
            if (currentCoords == null || currentCoords != coords)
            {
                result = scene.IsInsideSceneBoundaries(coords, position.y);
            }
            currentCoords = coords;
            isInsideScene = result;
            return result;
        }

        private void CheckSceneBoundaries(IDCLEntity entity)
        {
            sceneBoundsChecker?.AddEntityToBeChecked(entity);
            lastBoundariesCheckTime = Time.unscaledTime;
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