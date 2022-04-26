using System;
using UnityEngine;

namespace DCL.ECSRuntime.Tests
{
    [Serializable]
    public class TestingComponent
    {
        public Vector3 someVector;
        public string someString;
    }

    public static class TestingComponentSerialization
    {
        public static string Serialize(TestingComponent component)
        {
            return JsonUtility.ToJson(component);
        }

        public static TestingComponent Deserialize(object data)
        {
            return JsonUtility.FromJson<TestingComponent>(data as string);
        }
    }
}