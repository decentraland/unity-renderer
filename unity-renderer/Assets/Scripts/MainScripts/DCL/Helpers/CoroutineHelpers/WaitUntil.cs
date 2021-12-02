using System;
using UnityEngine;

namespace DCL
{
    public class WaitUntil : CustomYieldInstruction
    {
        Func<bool> predicate;
        float waitTime;
        bool waitEnabled = false;

        public WaitUntil(Func<bool> predicate, float? timeoutInSeconds = null)
        {
            this.predicate = predicate;

            if (timeoutInSeconds.HasValue)
            {
                waitEnabled = true;
                waitTime = Time.realtimeSinceStartup + timeoutInSeconds.Value;
            }
        }

        public override bool keepWaiting
        {
            get
            {
                if (waitEnabled)
                    return !predicate() && Time.realtimeSinceStartup < waitTime;
                else
                    return !predicate();
            }
        }
    }
}