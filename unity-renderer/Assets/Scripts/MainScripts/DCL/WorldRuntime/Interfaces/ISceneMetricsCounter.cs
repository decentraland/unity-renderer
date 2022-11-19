using System;
using DCL.Models;
using UnityEngine;

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

        void Configure(int sceneNumber, Vector2Int scenePosition, int sceneParcelCount);
    }
}