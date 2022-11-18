using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace DCL.Builder.Manifest
{
    [Serializable]
    public class WebBuilderScene
    {
        [FormerlySerializedAs("id")] public int sceneNumber;
        public Dictionary<string, BuilderEntity> entities = new Dictionary<string, BuilderEntity>();
        public Dictionary<string, BuilderComponent> components = new Dictionary<string, BuilderComponent>();
        public Dictionary<string, SceneObject> assets = new Dictionary<string, SceneObject>();

        public SceneMetricsModel metrics;
        public SceneMetricsModel limits;

        public BuilderGround ground;
    }
}