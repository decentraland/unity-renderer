using UnityEngine;

namespace DCL
{
    public class DataStore_Camera
    {
        public readonly BaseVariable<Quaternion> rotation = new BaseVariable<Quaternion>();
        public readonly BaseVariable<Transform> transform = new BaseVariable<Transform>();
        public readonly BaseVariable<bool> panning = new BaseVariable<bool>();
        public readonly BaseVariable<RenderTexture> outputTexture = new BaseVariable<RenderTexture>(null);
        public readonly BaseVariable<bool> invertYAxis = new BaseVariable<bool>();
        public readonly BaseVariable<Camera> hudsCamera = new BaseVariable<Camera>();
    }
}