using System;
using DCL.Models;

namespace DCL
{
    public interface ISceneMetricsCounter : IDisposable
    {
        event System.Action<ISceneMetricsCounter> OnMetricsUpdated;
        SceneMetricsModel ComputeSceneLimits();
        SceneMetricsModel GetModel();

        void Enable();

        void Disable();

        void SendEvent();

        bool IsInsideTheLimits();
        void RemoveExcludedEntity(string entityId);
        void AddExcludedEntity(string entityId);
    }
}