using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Builder.Manifest
{
    [Serializable]
    public class BuilderComponent
    {
        public string id;
        public string type;
        public object data;
    }
}