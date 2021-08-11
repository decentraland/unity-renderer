﻿using System;
using System.Collections.Generic;
using DCL.Controllers;
using DCL.Models;

namespace DCL.Components
{
    public interface ISharedComponent : IComponent, IDisposable
    {
        string id { get; }
        void AttachTo(IDCLEntity entity, Type overridenAttachedType = null);
        void DetachFrom(IDCLEntity entity, Type overridenAttachedType = null);
        void DetachFromEveryEntity();
        void Initialize(IParcelScene scene, string id);
        HashSet<IDCLEntity> GetAttachedEntities();
        void CallWhenReady(Action<ISharedComponent> callback);
    }
}