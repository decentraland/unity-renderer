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
        void SetFeedbackStyle(ISceneBoundsFeedbackStyle feedbackStyle);
        ISceneBoundsFeedbackStyle GetFeedbackStyle();
        List<Material> GetOriginalMaterials(MeshesInfo meshesInfo);
        void Start();
        void Stop();
        void AddEntityToBeChecked(IDCLEntity entity, bool isPersistent = false, bool runPreliminaryEvaluation = false);
        void RemoveEntity(IDCLEntity entity, bool removeIfPersistent = false, bool resetState = false);
        bool WasAddedAsPersistent(IDCLEntity entity);
        void RunEntityEvaluation(IDCLEntity entity);
        void RunEntityEvaluation(IDCLEntity entity, bool onlyOuterBoundsCheck);
        bool IsEntityMeshInsideSceneBoundaries(IDCLEntity entity);
    }
}