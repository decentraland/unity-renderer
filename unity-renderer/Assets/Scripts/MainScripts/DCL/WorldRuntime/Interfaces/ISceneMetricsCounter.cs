using System;
using DCL.Models;

namespace DCL
{
    public interface ISceneMetricsCounter : IDisposable
    {
        event System.Action<ISceneMetricsCounter> OnMetricsUpdated;
        SceneMetricsModel sceneLimits { get; }
        SceneMetricsModel model { get; }

        void AddEntity( string entityId );
        void RemoveEntity( string entityId );

        void Enable();

        void Disable();

        void SendEvent();

        bool IsInsideTheLimits();
        void RemoveExcludedEntity(string entityId);
        void AddExcludedEntity(string entityId);
    }
}