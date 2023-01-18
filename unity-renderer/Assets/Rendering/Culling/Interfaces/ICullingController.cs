using System;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Rendering
{
    public interface ICullingController : IService
    {
        void Start();
        void Restart();
        void Stop();
        void MarkDirty();
        void SetSettings(CullingControllerSettings settings);
        CullingControllerSettings GetSettingsCopy();
        void SetObjectCulling(bool enabled);
        void SetAnimationCulling(bool enabled);
        void SetShadowCulling(bool enabled);
        bool IsRunning();

        bool IsDirty();
        ICullingObjectsTracker objectsTracker { get; }

        delegate void DataReport(int rendererCount, int hiddenRendererCount, int hiddenShadowCount);

        event ICullingController.DataReport OnDataReport;
    }
}
