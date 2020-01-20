using UnityEngine;
using System.Runtime.InteropServices;

namespace Kongregate {
    public class WebGLMemoryStats : MonoBehaviour {
        [Tooltip("Interval (in seconds) between log entries")]
        public uint LogIntervalSeconds = 15;

        public static uint GetUsedMemorySize() {
            return GetTotalStackSize() + GetStaticMemorySize() + GetDynamicMemorySize();
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        public static uint GetFreeMemorySize() {
            return GetTotalMemorySize() - GetUsedMemorySize();
        }

        void Start() {
            InvokeRepeating("Log", 0, LogIntervalSeconds);
        }

        private void Log() {
            var total = GetTotalMemorySize() / 1024 / 1024;
            var used = GetUsedMemorySize() / 1024 / 1024;
            var free = GetFreeMemorySize() / 1024 / 1024;
            Debug.Log(string.Format("WebGL Memory - Total: {0}MB, Used: {1}MB, Free: {2}MB", total, used, free));
        }

        [DllImport("__Internal")]
        public static extern uint GetTotalMemorySize();

        [DllImport("__Internal")]
        public static extern uint GetTotalStackSize();

        [DllImport("__Internal")]
        public static extern uint GetStaticMemorySize();

        [DllImport("__Internal")]
        public static extern uint GetDynamicMemorySize();
#else
        public static uint GetFreeMemorySize() { return 0; }
        public static uint GetTotalMemorySize() { return 0; }
        public static uint GetTotalStackSize() { return 0; }
        public static uint GetStaticMemorySize() { return 0; }
        public static uint GetDynamicMemorySize() { return 0; }
#endif
    }
}
