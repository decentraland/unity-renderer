using GLTFast.Logging;
using UnityEngine;

namespace DCL
{
    internal class GLTFImportLogger : ICodeLogger
    {
        private string lastError = "";
        
        public string GetLastError() => lastError;
        public void Error(LogCode code, params string[] messages)
        {
            lastError = $"{code} {string.Join("\n", messages)}";
            Debug.LogError(lastError);
        }
        public void Warning(LogCode code, params string[] messages) {  Debug.LogWarning($"{code} {string.Join("\n", messages)}"); }
        public void Info(LogCode code, params string[] messages) { Debug.Log($"{code} {string.Join("\n", messages)}"); }
        public void Error(string message) { Debug.LogError(message); }
        public void Warning(string message) { Debug.LogWarning(message); }
        public void Info(string message) { Debug.Log(message); }
    }
}