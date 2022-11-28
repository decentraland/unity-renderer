using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Altom.AltDriver;
using Altom.AltDriver.Commands;
using Altom.AltTester.Commands;
using Altom.AltTester.Logging;
using Newtonsoft.Json;

namespace Altom.AltTester.Communication
{
    public class CommandHandler : ICommandHandler
    {
        private static readonly NLog.Logger logger = ServerLogManager.Instance.GetCurrentClassLogger();

        public CommandHandler()
        {
        }

        public SendMessageHandler OnSendMessage { get; set; }

        public void Send(string data)
        {
            if (this.OnSendMessage != null)
            {
                this.OnSendMessage.Invoke(data);
                logger.Debug("response sent: " + trimLog(data));
            }
        }

        public void OnMessage(string data)
        {
            logger.Debug("command received: " + trimLog(data));

            var jsonSerializerSettings = new JsonSerializerSettings
            {
                Culture = CultureInfo.InvariantCulture
            };
            Func<string> executeAndSerialize = null;
            CommandParams cmdParams = null;

            try
            {
                cmdParams = JsonConvert.DeserializeObject<CommandParams>(data, jsonSerializerSettings);
                var type = getCommandType((string)cmdParams.commandName);
                var commandParams = JsonConvert.DeserializeObject(data, type, jsonSerializerSettings) as CommandParams;
                executeAndSerialize = createCommand(commandParams);
            }
            catch (JsonException ex)
            {
                executeAndSerialize = new AltInvalidCommand(cmdParams, ex).ExecuteAndSerialize;
            }
            catch (CommandNotFoundException ex)
            {
                executeAndSerialize = new AltInvalidCommand(cmdParams, ex).ExecuteAndSerialize;
            }

            AltRunner._responseQueue.ScheduleResponse(delegate
                {

                    var response = executeAndSerialize();
                    //TODO: remove this
                    if (!cmdParams.commandName.Equals("endTouch")) //Temporary solution to ignore first "Ok"
                    {
                        //Do not remove the send only the if
                        this.Send(response);
                    }
                });
        }

        private string trimLog(string log, int maxLogLength = 1000)
        {
            if (string.IsNullOrEmpty(log)) return log;
            if (log.Length <= maxLogLength) return log;
            return log.Substring(0, maxLogLength) + "[...]";
        }

        private Func<string> createCommand(CommandParams cmdParams)
        {
            if (cmdParams is AltGetServerVersionParams)
            {
                return new AltGetServerVersionCommand((AltGetServerVersionParams)cmdParams).ExecuteAndSerialize;
            }
            if (cmdParams is AltTapElementParams)
            {
                return new AltTapElementCommand(this, cmdParams as AltTapElementParams).ExecuteAndSerialize;
            }
            if (cmdParams is AltClickElementParams)
            {
                return new AltClickElementCommand(this, cmdParams as AltClickElementParams).ExecuteAndSerialize;
            }
            if (cmdParams is AltTapCoordinatesParams)
            {
                return new AltTapCoordinatesCommand(this, cmdParams as AltTapCoordinatesParams).ExecuteAndSerialize;
            }
            if (cmdParams is AltClickCoordinatesParams)
            {
                return new AltClickCoordinatesCommand(this, cmdParams as AltClickCoordinatesParams).ExecuteAndSerialize;
            }
            if (cmdParams is AltKeysDownParams)
            {
                return new AltKeysDownCommand(cmdParams as AltKeysDownParams).ExecuteAndSerialize;
            }
            if (cmdParams is AltKeysUpParams)
            {
                return new AltKeysUpCommand(cmdParams as AltKeysUpParams).ExecuteAndSerialize;
            }
            if (cmdParams is AltBeginTouchParams)
            {
                return new AltBeginTouchCommand(cmdParams as AltBeginTouchParams).ExecuteAndSerialize;
            }
            if (cmdParams is AltMoveTouchParams)
            {
                return new AltMoveTouchCommand(cmdParams as AltMoveTouchParams).ExecuteAndSerialize;
            }
            if (cmdParams is AltEndTouchParams)
            {
                return new AltEndTouchCommand(this, cmdParams as AltEndTouchParams).ExecuteAndSerialize;
            }
            if (cmdParams is AltGetCurrentSceneParams)
            {
                return new AltGetCurrentSceneCommand((AltGetCurrentSceneParams)cmdParams).ExecuteAndSerialize;
            }
            if (cmdParams is AltGetObjectComponentPropertyParams)
            {
                return new AltGetComponentPropertyCommand(cmdParams as AltGetObjectComponentPropertyParams).ExecuteAndSerialize;
            }
            if (cmdParams is AltSetObjectComponentPropertyParams)
            {
                return new AltSetComponentPropertyCommand(cmdParams as AltSetObjectComponentPropertyParams).ExecuteAndSerialize;
            }
            if (cmdParams is AltCallComponentMethodForObjectParams)
            {
                return new AltCallComponentMethodForObjectCommand(cmdParams as AltCallComponentMethodForObjectParams).ExecuteAndSerialize;
            }
            if (cmdParams is AltDragObjectParams)
            {
                return new AltDragObjectCommand(cmdParams as AltDragObjectParams).ExecuteAndSerialize;
            }
            if (cmdParams is AltPointerUpFromObjectParams)
            {
                return new AltPointerUpFromObjectCommand(cmdParams as AltPointerUpFromObjectParams).ExecuteAndSerialize;
            }
            if (cmdParams is AltPointerDownFromObjectParams)
            {
                return new AltPointerDownFromObjectCommand(cmdParams as AltPointerDownFromObjectParams).ExecuteAndSerialize;
            }
            if (cmdParams is AltPointerEnterObjectParams)
            {
                return new AltPointerEnterObjectCommand(cmdParams as AltPointerEnterObjectParams).ExecuteAndSerialize;
            }
            if (cmdParams is AltPointerExitObjectParams)
            {
                return new AltPointerExitObjectCommand(cmdParams as AltPointerExitObjectParams).ExecuteAndSerialize;
            }
            if (cmdParams is AltTiltParams)
            {
                return new AltTiltCommand(this, cmdParams as AltTiltParams).ExecuteAndSerialize;
            }
            if (cmdParams is AltSwipeParams)
            {
                return new AltSwipeCommand(this, cmdParams as AltSwipeParams).ExecuteAndSerialize;
            }
            if (cmdParams is AltMultipointSwipeParams)
            {
                return new AltMultipointSwipeCommand(this, cmdParams as AltMultipointSwipeParams).ExecuteAndSerialize;
            }
            if (cmdParams is AltLoadSceneParams)
            {
                return new AltLoadSceneCommand(this, (AltLoadSceneParams)cmdParams).ExecuteAndSerialize;
            }
            if (cmdParams is AltUnloadSceneParams)
            {
                return new AltUnloadSceneCommand(this, cmdParams as AltUnloadSceneParams).ExecuteAndSerialize;
            }
            if (cmdParams is AltSetTimeScaleParams)
            {
                return new AltSetTimeScaleCommand(cmdParams as AltSetTimeScaleParams).ExecuteAndSerialize;
            }
            if (cmdParams is AltGetTimeScaleParams)
            {
                return new AltGetTimeScaleCommand(cmdParams as AltGetTimeScaleParams).ExecuteAndSerialize;
            }
            if (cmdParams is AltDeletePlayerPrefParams)
            {
                return new AltDeletePlayerPrefCommand(cmdParams as AltDeletePlayerPrefParams).ExecuteAndSerialize;
            }
            if (cmdParams is AltDeleteKeyPlayerPrefParams)
            {
                return new AltDeleteKeyPlayerPrefCommand(cmdParams as AltDeleteKeyPlayerPrefParams).ExecuteAndSerialize;
            }
            if (cmdParams is AltSetKeyPlayerPrefParams)
            {
                return new AltSetKeyPlayerPrefCommand(cmdParams as AltSetKeyPlayerPrefParams).ExecuteAndSerialize;
            }
            if (cmdParams is AltGetKeyPlayerPrefParams)
            {
                var getKeyPlayerPrefParams = cmdParams as AltGetKeyPlayerPrefParams;
                switch (getKeyPlayerPrefParams.keyType)
                {
                    case PlayerPrefKeyType.Int: return new AltGetIntKeyPlayerPrefCommand(cmdParams as AltGetKeyPlayerPrefParams).ExecuteAndSerialize;
                    case PlayerPrefKeyType.String: return new AltGetStringKeyPlayerPrefCommand(cmdParams as AltGetKeyPlayerPrefParams).ExecuteAndSerialize;
                    case PlayerPrefKeyType.Float: return new AltGetFloatKeyPlayerPrefCommand(cmdParams as AltGetKeyPlayerPrefParams).ExecuteAndSerialize;
                    default:
                        return new AltInvalidCommand(cmdParams, new InvalidParameterTypeException(string.Format("PlayerPrefKeyType {0} not handled", getKeyPlayerPrefParams.keyType))).ExecuteAndSerialize;
                }
            }

            if (cmdParams is AltGetAllComponentsParams)
            {
                return new AltGetAllComponentsCommand(cmdParams as AltGetAllComponentsParams).ExecuteAndSerialize;
            }
            if (cmdParams is AltGetAllFieldsParams)
            {
                return new AltGetAllFieldsCommand(cmdParams as AltGetAllFieldsParams).ExecuteAndSerialize;
            }
            if (cmdParams is AltGetAllPropertiesParams)
            {
                return new AltGetAllPropertiesCommand(cmdParams as AltGetAllPropertiesParams).ExecuteAndSerialize;
            }
            if (cmdParams is AltGetAllMethodsParams)
            {
                return new AltGetAllMethodsCommand(cmdParams as AltGetAllMethodsParams).ExecuteAndSerialize;
            }
            if (cmdParams is AltGetAllScenesParams)
            {
                return new AltGetAllScenesCommand(cmdParams as AltGetAllScenesParams).ExecuteAndSerialize;
            }
            if (cmdParams is AltGetAllCamerasParams)
            {
                return new AltGetAllCamerasCommand(cmdParams as AltGetAllCamerasParams).ExecuteAndSerialize;
            }
            if (cmdParams is AltGetAllActiveCamerasParams)
            {
                return new AltGetAllCamerasCommand(cmdParams as AltGetAllActiveCamerasParams).ExecuteAndSerialize;
            }
            if (cmdParams is AltGetAllLoadedScenesParams)
            {
                return new AltGetAllLoadedScenesCommand(cmdParams as AltGetAllLoadedScenesParams).ExecuteAndSerialize;
            }
            if (cmdParams is AltGetAllLoadedScenesAndObjectsParams)
            {
                return new AltGetAllLoadedScenesAndObjectsCommand(cmdParams as AltGetAllLoadedScenesAndObjectsParams).ExecuteAndSerialize;
            }
            if (cmdParams is AltGetScreenshotParams)
            {
                return new AltGetScreenshotCommand(this, cmdParams as AltGetScreenshotParams).ExecuteAndSerialize;
            }
            if (cmdParams is AltHighlightObjectScreenshotParams)
            {
                return new AltHighlightSelectedObjectCommand(this, cmdParams as AltHighlightObjectScreenshotParams).ExecuteAndSerialize;
            }
            if (cmdParams is AltHighlightObjectFromCoordinatesScreenshotParams)
            {
                return new AltHighlightObjectFromCoordinatesCommand(this, cmdParams as AltHighlightObjectFromCoordinatesScreenshotParams).ExecuteAndSerialize;
            }
            if (cmdParams is AltPressKeyboardKeysParams)
            {
                return new AltPressKeyboardKeysCommand(this, cmdParams as AltPressKeyboardKeysParams).ExecuteAndSerialize;
            }
            if (cmdParams is AltMoveMouseParams)
            {
                return new AltMoveMouseCommand(this, cmdParams as AltMoveMouseParams).ExecuteAndSerialize;
            }
            if (cmdParams is AltScrollParams)
            {
                return new AltScrollCommand(this, cmdParams as AltScrollParams).ExecuteAndSerialize;
            }
            if (cmdParams is AltFindObjectParams)
            {
                return new AltFindObjectCommand(cmdParams as AltFindObjectParams).ExecuteAndSerialize;
            }
            if (cmdParams is AltFindObjectsParams)
            {
                return new AltFindObjectsCommand(cmdParams as AltFindObjectsParams).ExecuteAndSerialize;
            }
            if (cmdParams is AltFindObjectsLightParams)
            {
                return new AltFindObjectsLightCommand(cmdParams as AltFindObjectsLightParams).ExecuteAndSerialize;
            }
            if (cmdParams is AltGetTextParams)
            {
                return new AltGetTextCommand(cmdParams as AltGetTextParams).ExecuteAndSerialize;
            }
            if (cmdParams is AltSetTextParams)
            {
                return new AltSetTextCommand(cmdParams as AltSetTextParams).ExecuteAndSerialize;
            }
            if (cmdParams is AltGetPNGScreenshotParams)
            {
                return new AltGetScreenshotPNGCommand(this, cmdParams as AltGetPNGScreenshotParams).ExecuteAndSerialize;
            }
            if (cmdParams is AltGetServerVersionParams)
            {
                return new AltGetServerVersionCommand(cmdParams as AltGetServerVersionParams).ExecuteAndSerialize;
            }
            if (cmdParams is AltSetServerLoggingParams)
            {
                return new AltSetServerLoggingCommand(cmdParams as AltSetServerLoggingParams).ExecuteAndSerialize;
            }
            if (cmdParams is ActivateNotification)
            {
                return new AltActivateNotificationCommand(this, cmdParams as ActivateNotification).ExecuteAndSerialize;
            }
            if (cmdParams is DeactivateNotification)
            {
                return new AltDeactivateNotificationCommand(this, cmdParams as DeactivateNotification).ExecuteAndSerialize;
            }
            if (cmdParams is AltFindObjectAtCoordinatesParams)
            {
                return new AltFindObjectAtCoordinatesCommand(cmdParams as AltFindObjectAtCoordinatesParams).ExecuteAndSerialize;
            }

            return new AltInvalidCommand(cmdParams, new CommandNotFoundException(string.Format("Command {0} not handled", cmdParams.commandName))).ExecuteAndSerialize;
        }

        private Type getCommandType(string commandName)
        {
            var assembly = Assembly.GetAssembly(typeof(CommandParams));

            var derivedType = typeof(CommandParams);
            var type = assembly.GetTypes().FirstOrDefault(t =>
               {
                   if (derivedType.IsAssignableFrom(t)) // if type derrives from CommandParams
                   {
                       CommandAttribute cmdAttribute = (CommandAttribute)Attribute.GetCustomAttribute(t, typeof(CommandAttribute));
                       return cmdAttribute != null && cmdAttribute.Name == commandName;
                   }
                   return false;
               });

            if (type == null) { throw new CommandNotFoundException(string.Format("Command `{0}` not found", commandName)); }
            return type;
        }
    }
}
