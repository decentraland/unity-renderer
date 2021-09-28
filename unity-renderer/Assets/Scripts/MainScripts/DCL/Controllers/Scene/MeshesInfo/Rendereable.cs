using System;
using NSubstitute.Extensions;
using UnityEngine;
using UnityEngine.Windows.WebCam;

namespace DCL
{
    /// <summary>
    /// The Rendereable object represents any loaded object that should be visible in the world.
    /// 
    /// In the future, we may want to add a Renderer[] list here.
    ///
    /// With this in place, the SceneBoundsChecker and CullingController implementations can
    /// be changed to be reactive, and lots of FindObjects and GetComponentsInChildren calls can be
    /// saved.
    /// </summary>
    public class Rendereable : ICloneable
    {
        public int triangleCount = 0;
        public GameObject container;

        public bool Equals(Rendereable other)
        {
            return triangleCount == other.triangleCount &&
                   container == other.container;
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}