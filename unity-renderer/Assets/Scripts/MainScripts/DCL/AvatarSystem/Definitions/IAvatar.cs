using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace AvatarSystem
{
    public interface IAvatar : IDisposable
    {
        UniTask Load(List<string> wearablesIds, AvatarSettings settings, CancellationToken ct = default);
        void SetVisibility(bool visible);
        void SetExpression(string expressionId, long timestamps);
    }

    public interface IVisibility
    {
        bool composedVisibility { get; }
        void SetVisible(bool visible);
    }
}