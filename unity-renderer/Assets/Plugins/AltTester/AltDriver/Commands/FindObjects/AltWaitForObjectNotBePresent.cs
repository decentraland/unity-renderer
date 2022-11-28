using System;
using System.Threading;
using Altom.AltDriver.Logging;

namespace Altom.AltDriver.Commands
{
    public class AltWaitForObjectNotBePresent : AltBaseFindObjects
    {
        readonly NLog.Logger logger = DriverLogManager.Instance.GetCurrentClassLogger();
        AltFindObject findObject;
        private readonly string path;
        double timeout;
        double interval;
        public AltWaitForObjectNotBePresent(IDriverCommunication commHandler, By by, string value, By cameraBy, string cameraValue, bool enabled, double timeout, double interval) : base(commHandler)
        {
            findObject = new AltFindObject(commHandler, by, value, cameraBy, cameraValue, enabled);
            path = SetPath(by, value);

            this.timeout = timeout;
            this.interval = interval;
            if (timeout <= 0) throw new ArgumentOutOfRangeException("timeout");
            if (interval <= 0) throw new ArgumentOutOfRangeException("interval");
        }
        public void Execute()
        {
            double time = 0;
            bool found = false;
            AltObject altElement;

            logger.Debug("Waiting for element " + path + " to not be present");
            while (time < timeout)
            {
                found = false;
                try
                {
                    altElement = findObject.Execute();
                    found = true;
                    Thread.Sleep(System.Convert.ToInt32(interval * 1000));
                    time += interval;

                }
                catch (NotFoundException)
                {
                    break;
                }
            }
            if (found)
                throw new WaitTimeOutException("Element " + path + " still found after " + timeout + " seconds");
        }
    }
}