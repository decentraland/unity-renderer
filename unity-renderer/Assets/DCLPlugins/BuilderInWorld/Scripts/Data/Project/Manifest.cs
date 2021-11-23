using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Builder.Manifest
{
    [Serializable]
    public class Manifest
    {
        public int version;
        public ProjectData project;
        public BuilderScene scene;
    }
}