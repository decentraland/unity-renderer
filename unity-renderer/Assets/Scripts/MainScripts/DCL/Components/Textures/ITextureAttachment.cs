using System;
using UnityEngine;

namespace DCL.Components
{
    public interface ITextureAttachment : IDisposable
    {
        ISharedComponent component { get; }
        event System.Action<ITextureAttachment> OnUpdate;
        event System.Action<ITextureAttachment> OnDetach;
        event System.Action<ITextureAttachment> OnAttach;
        float GetClosestDistanceSqr(Vector3 fromPosition);
        bool IsVisible();
        string GetId();
    }
}