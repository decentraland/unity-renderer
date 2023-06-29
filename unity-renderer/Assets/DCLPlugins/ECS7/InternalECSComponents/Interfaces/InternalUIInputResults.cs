using DCL.ECS7.ComponentWrapper;
using System.Collections.Generic;

namespace DCL.ECS7.InternalComponents
{
    public class InternalUIInputResults : InternalComponent
    {
        public readonly struct Result
        {
            public readonly IPooledWrappedComponent Message;
            public readonly int ComponentId;

            public Result(IPooledWrappedComponent message, int componentId)
            {
                this.Message = message;
                this.ComponentId = componentId;
            }
        }

        public readonly Queue<Result> Results = new ();
    }
}
