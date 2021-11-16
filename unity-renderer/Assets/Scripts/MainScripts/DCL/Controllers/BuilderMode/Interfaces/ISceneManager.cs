using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Builder
{
    public interface ISceneManager
    {
        void Initialize(IContext context);
        void Dispose();
        void Update();

    }
}