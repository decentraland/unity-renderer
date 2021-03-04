using UnityEngine;

namespace DCL
{
    public class CleanableYieldInstruction : CustomYieldInstruction, ICleanable
    {
        public virtual void Cleanup()
        {
        }

        public override bool keepWaiting
        {
            get { return false; }
        }
    }
}