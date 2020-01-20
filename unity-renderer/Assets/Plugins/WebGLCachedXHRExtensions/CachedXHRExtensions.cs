using UnityEngine;
using System.Runtime.InteropServices;

namespace Kongregate {
    public class CacheEntryQuery : CustomYieldInstruction {
        private readonly string Url;
        private int Status = 0;

        public CacheEntryQuery(string url) {
            Url = url;
            CachedXHRExtensions_SearchCache(url);
        }

        public override bool keepWaiting {
            get {
                Status = CachedXHRExtensions_CheckStatus(Url);
                return Status == 0;
            }
        }

        public bool IsCached {
            get {
                if (Status == 0) {
                    Debug.Log("CacheEntryQuery: returning IsCached=false since query is pending");
                    return false;
                }

                return Status == 1;
            }
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void CachedXHRExtensions_SearchCache(string url);

        [DllImport("__Internal")]
        private static extern int CachedXHRExtensions_CheckStatus(string url);
#else
        private static void CachedXHRExtensions_SearchCache(string url) { }
        private static int CachedXHRExtensions_CheckStatus(string url) { return -1; }
#endif
    }

    public class CachedXHRExtensions {
        public static void CleanCache() {
            CachedXHRExtensions_CleanCache();
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        static extern int CachedXHRExtensions_CleanCache();
#else
        static void CachedXHRExtensions_CleanCache() {}
#endif
    }
}
