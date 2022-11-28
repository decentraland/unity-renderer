using System.Globalization;
using Altom.AltDriver.Commands;
using Altom.AltTester.Communication;
using Newtonsoft.Json;

namespace Altom.AltTester.Notification
{
    public class BaseNotification
    {
        private static ICommandHandler commandHandler;

        public BaseNotification(ICommandHandler commandHandlerParam)
        {
            commandHandler = commandHandlerParam;
        }

        protected static void SendNotification<T>(T data, string commandName)
        {
            var cmdResponse = new CommandResponse
            {
                commandName = commandName,
                messageId = null,
                data = JsonConvert.SerializeObject(data, new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    Culture = CultureInfo.InvariantCulture
                }),
                error = null,
                isNotification = true
            };

            var notification = JsonConvert.SerializeObject(cmdResponse, new JsonSerializerSettings
            {
                Culture = CultureInfo.InvariantCulture
            });
            commandHandler.Send(notification);

        }
    }
}