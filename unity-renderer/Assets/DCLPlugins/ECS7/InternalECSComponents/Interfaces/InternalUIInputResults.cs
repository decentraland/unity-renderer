using Google.Protobuf;
using System.Collections.Generic;

namespace DCL.ECS7.InternalComponents
{
    public class InternalUIInputResults : InternalComponent
    {
        public readonly struct Result
        {
            public readonly IMessage Message;
            public readonly int ComponentId;

            public Result(IMessage message, int componentId)
            {
                this.Message = message;
                this.ComponentId = componentId;
            }
        }

        public readonly Queue<Result> Results = new ();
    }
}
