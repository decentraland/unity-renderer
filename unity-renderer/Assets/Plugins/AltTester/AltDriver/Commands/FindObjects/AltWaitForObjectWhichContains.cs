using System;
using System.Threading;
using Altom.AltDriver.Logging;

namespace Altom.AltDriver.Commands
{
    public class AltWaitForObjectWhichContains : AltBaseFindObjects
    {
        readonly NLog.Logger logger = DriverLogManager.Instance.GetCurrentClassLogger();
        private AltFindObjectWhichContains findObject;
        private readonly string path;
        private readonly double timeout;
        private readonly double interval;

        public AltWaitForObjectWhichContains(IDriverCommunication commHandler, By by, string value, By cameraBy, string cameraValue, bool enabled, double timeout, double interval) : base(commHandler)
        {
            path = SetPath(by, value);
            if (timeout <= 0) throw new ArgumentOutOfRangeException("timeout");
            if (interval <= 0) throw new ArgumentOutOfRangeException("interval");
            this.timeout = timeout;
            this.interval = interval;
            findObject = new AltFindObjectWhichContains(CommHandler, by, value, cameraBy, cameraValue, enabled);
        }
        public AltObject Execute()
        {
            double time = 0;
            AltObject altElement = null;

            logger.Debug("Waiting for element " + path + " to be present.");
            while (time < timeout)
            {
                try
                {
                    altElement = findObject.Execute();
                    break;
                }
                catch (NotFoundException)
                {
                    Thread.Sleep(System.Convert.ToInt32(interval * 1000));
                    time += interval;
                }
            }
            if (altElement != null)
                return altElement;
            throw new WaitTimeOutException("Element " + path + " not loaded after " + timeout + " seconds");
        }
    }
}