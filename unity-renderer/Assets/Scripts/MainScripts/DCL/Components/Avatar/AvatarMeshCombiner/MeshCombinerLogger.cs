using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace DCL
{
    /// <summary>
    /// Remove any calls from compilation to prevent string allocations
    /// </summary>
    public static class MeshCombinerLogger
    {
        public const string COMPILATION_DEFINE = "MESH_COMBINER_VERBOSE";

        [Conditional(COMPILATION_DEFINE)]
        public static void Log(object message)
        {
            Debug.Log(message);
        }

        [Conditional(COMPILATION_DEFINE)]
        public static void Log(object message, Object context)
        {
            Debug.Log(message, context);
        }

        /// <summary>
        /// Warnings are not omitted
        /// </summary>
        public static void LogWarning(object message)
        {
            Debug.LogWarning(message);
        }

        public static void LogError(object message)
        {
            Debug.LogError(message);
        }
    }
}
