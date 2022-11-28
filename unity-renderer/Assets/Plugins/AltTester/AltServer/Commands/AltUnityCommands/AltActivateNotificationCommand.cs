using System;
using Altom.AltDriver.Commands;
using Altom.AltDriver.Notifications;
using Altom.AltTester.Communication;
using Altom.AltTester.Notification;

namespace Altom.AltTester.Commands
{
    public class AltActivateNotificationCommand : AltCommand<ActivateNotification, string>
    {
        ICommandHandler commandHandler;
        public AltActivateNotificationCommand(ICommandHandler commandHandler, ActivateNotification cmdParams) : base(cmdParams)
        {
            this.commandHandler = commandHandler;

        }
        public override string Execute()
        {
            switch (CommandParams.NotificationType)
            {
                case NotificationType.LOADSCENE:
                    new AltLoadSceneNotification(commandHandler, true);
                    break;
                case NotificationType.UNLOADSCENE:
                    new AltUnloadSceneNotification(commandHandler, true);
                    break;
                case NotificationType.LOG:
                    new AltLogNotification(commandHandler, true);
                    break;
                case NotificationType.APPLICATION_PAUSED:
                    new AltTesterApplicationPausedNotification(commandHandler, true);
                    break;
            }
            return "Ok";
        }

    }
}