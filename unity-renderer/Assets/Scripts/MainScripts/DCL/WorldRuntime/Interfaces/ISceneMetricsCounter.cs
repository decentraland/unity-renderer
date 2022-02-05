using System;
using DCL.Models;

namespace DCL
{
    public interface ISceneMetricsCounter : IDisposable
    {
        event System.Action<ISceneMetricsCounter> OnMetricsUpdated;
        SceneMetricsModel maxCount { get; }
        SceneMetricsModel currentCount { get; }

        void Enable();

        void Disable();

        void SendEvent();

        bool IsInsideTheLimits();
    }
}