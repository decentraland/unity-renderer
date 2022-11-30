#if UNITY_EDITOR || ALTTESTER
using NLog;
using NLog.Targets;
using UnityEngine;

namespace Altom.AltDriver.Logging
{
    /// <summary> An appender which logs to the unity console. </summary>
    public class UnityTarget : TargetWithLayout
    {
        public UnityTarget(string name)
        {
            this.Name = name;
        }
        /// <inheritdoc />
        protected override void Write(LogEventInfo logEvent)
        {
            string message = this.Layout.Render(logEvent);

            if (logEvent.Level >= LogLevel.Error)
            {
                // everything above or equal to error is an error
                Debug.LogError(message);
            }
            else if (logEvent.Level >= LogLevel.Warn)
            {
                // everything that is a warning up to error is logged as warning
                Debug.LogWarning(message);
            }
            else
            {
                // everything else we'll just log normally
                Debug.Log(message);
            }
        }
    }

}
#endif