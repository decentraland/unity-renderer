using GLTFast.Logging;
using UnityEngine;

namespace DCL
{
    internal class GLTFImportLogger : ICodeLogger
    {
        public void Error(LogCode code, params string[] messages) { Debug.LogError(string.Join("\n", messages)); }
        public void Warning(LogCode code, params string[] messages) {  Debug.LogWarning(string.Join("\n", messages)); }
        public void Info(LogCode code, params string[] messages) { Debug.Log(string.Join("\n", messages)); }
        public void Error(string message) { Debug.LogError(message); }
        public void Warning(string message) { Debug.LogWarning(message); }
        public void Info(string message) { Debug.Log(message); }
    }
}