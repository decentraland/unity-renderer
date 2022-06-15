using System.Collections.Generic;
using DCL;

namespace RPC.Context
{
    public class CRDTServiceContext
    {
        public readonly Queue<(string, byte[])> notifications = new Queue<(string, byte[])>();
        public IMessageQueueHandler messageQueueHanlder;
    }
}