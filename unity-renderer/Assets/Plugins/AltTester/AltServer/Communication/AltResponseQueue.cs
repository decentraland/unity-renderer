using System.Collections.Generic;

namespace Altom.AltTester.Communication
{
    public delegate void SendResponse();

    public class AltResponseQueue
    {
        private readonly Queue<SendResponse> responseQueue = new Queue<SendResponse>();
        private readonly object queueLock = new object();

        public void Cycle()
        {
            if (responseQueue.Count == 0) return;
            lock (queueLock)
            {
                if (responseQueue.Count > 0)
                {
                    responseQueue.Dequeue()();
                }
            }
        }

        public void ScheduleResponse(SendResponse newResponse)
        {
            lock (queueLock)
            {
                if (responseQueue.Count < 100)
                {
                    responseQueue.Enqueue(newResponse);
                }
            }
        }
    }
}


