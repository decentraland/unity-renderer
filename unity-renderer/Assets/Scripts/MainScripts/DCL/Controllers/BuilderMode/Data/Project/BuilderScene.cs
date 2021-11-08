using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Builder.Manifest
{
    [Serializable]
    public class BuilderScene
    {
        public string id;
        public Dictionary<string, BuilderEntity> entities;
        public Dictionary<string, BuilderComponent> components;
        public Dictionary<string, SceneObject> assets;

        public SceneMetricsModel metrics;
        public SceneMetricsModel limits;

        public BuilderGround ground;

    }
}