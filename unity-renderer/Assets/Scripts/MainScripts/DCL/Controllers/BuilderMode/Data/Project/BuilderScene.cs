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
        public Dictionary<string, BuilderEntity> entities = new Dictionary<string, BuilderEntity>();
        public Dictionary<string, BuilderComponent> components = new Dictionary<string, BuilderComponent>();
        public Dictionary<string, SceneObject> assets = new Dictionary<string, SceneObject>();

        public SceneMetricsModel metrics;
        public SceneMetricsModel limits;

        public BuilderGround ground;
    }
}