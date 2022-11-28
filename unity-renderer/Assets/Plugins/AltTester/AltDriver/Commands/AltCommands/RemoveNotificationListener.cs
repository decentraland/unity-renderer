using System;
using Altom.AltDriver.Notifications;

namespace Altom.AltDriver.Commands
{
    public class RemoveNotificationListener : AltBaseCommand
    {
        private readonly DeactivateNotification cmdParams;

        public RemoveNotificationListener(IDriverCommunication commHandler, NotificationType notificationType) : base(commHandler)
        {
            this.cmdParams = new DeactivateNotification(notificationType);
        }
        public void Execute()
        {
            this.CommHandler.RemoveNotificationListener(cmdParams.NotificationType);
            this.CommHandler.Send(this.cmdParams);
            var data = this.CommHandler.Recvall<string>(this.cmdParams);
            ValidateResponse("Ok", data);
        }
    }
}
