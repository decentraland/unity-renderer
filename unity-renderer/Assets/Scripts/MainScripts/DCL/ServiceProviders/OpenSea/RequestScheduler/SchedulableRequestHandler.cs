using MainScripts.DCL.ServiceProviders.OpenSea.RequestHandlers;
using System;

namespace MainScripts.DCL.ServiceProviders.OpenSea.RequestScheduler
{
    internal class SchedulableRequestHandler
    {
        public event Action<IRequestHandler> OnReadyToSchedule;
        public bool isReadyToSchedule { private set; get; }

        public void SetReadyToBeScheduled(IRequestHandler handler)
        {
            isReadyToSchedule = true;
            OnReadyToSchedule?.Invoke(handler);
        }
    }
}