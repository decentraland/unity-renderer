using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Skybox
{
    [CreateAssetMenu(fileName = "SkyboxMaterialData", menuName = "ScriptableObjects/SkyboxMaterialData", order = 1)]
    public class MaterialReferenceContainer : ScriptableObject
    {
        private static MaterialReferenceContainer instance;
        public static MaterialReferenceContainer i => GetOrLoad(ref instance, "Skybox Materials/SkyboxMaterialData");

        public Material skyboxMat;
        public int skyboxMatSlots = 5;
        public Material domeMat;

        private static T GetOrLoad<T>(ref T variable, string path) where T : Object
        {
            if (variable == null)
            {
                variable = Resources.Load<T>(path);
            }

            return variable;
        }

        public Material GetSkyboxMaterial() { return skyboxMat; }

        public Material GetDomeMaterial() { return domeMat; }
    }
}