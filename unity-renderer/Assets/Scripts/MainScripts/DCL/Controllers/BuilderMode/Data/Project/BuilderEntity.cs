using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Builder.Manifest
{
    [Serializable]
    public class BuilderEntity
    {
        public string id;
        public string[] components;
        public bool disableGizmos;
        public string name;
    }
}