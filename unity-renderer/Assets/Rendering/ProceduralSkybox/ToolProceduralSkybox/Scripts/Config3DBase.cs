using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Skybox
{
    public enum Additional3DElements
    {
        Dome,
        Satellite
    }

    [System.Serializable]
    public class Config3DBase
    {
        public Additional3DElements configType;
    }
}