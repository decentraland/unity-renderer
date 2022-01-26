using System;
using System.Collections.Generic;
using DCL.Models;
using UnityEngine;

namespace DCL.Controllers
{
    public interface ISceneBoundsChecker : IService
    {
        event Action<IDCLEntity, bool> OnEntityBoundsCheckerStatusChanged;

        float timeBetweenChecks { get; set; }
        bool enabled { get; }
        int entitiesToCheckCount { get; }
        int highPrioEntitiesToCheckCount { get; }
        void SetFeedbackStyle(ISceneBoundsFeedbackStyle feedbackStyle);
        ISceneBoundsFeedbackStyle GetFeedbackStyle();
        List<Material> GetOriginalMaterials(MeshesInfo meshesInfo);
        void Start();
        void Stop();
        void AddEntityToBeChecked(IDCLEntity entity);

        /// <summary>
        /// Add an entity that will be consistently checked, until manually removed from the list.
        /// </summary>
        void AddPersistent(IDCLEntity entity);

        /// <summary>
        /// Returns whether an entity was added to be consistently checked
        /// </summary>
        ///
        bool WasAddedAsPersistent(IDCLEntity entity);

        void RemoveEntityToBeChecked(IDCLEntity entity);
        void EvaluateEntityPosition(IDCLEntity entity);
        bool IsEntityInsideSceneBoundaries(IDCLEntity entity);
    }
}