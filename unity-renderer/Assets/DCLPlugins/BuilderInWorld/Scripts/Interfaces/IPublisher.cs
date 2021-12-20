using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Builder
{
    public interface IPublisher
    {
        void Initialize(IContext context);
        void Dipose();
        void StartPublish(IBuilderScene scene);
    }
}