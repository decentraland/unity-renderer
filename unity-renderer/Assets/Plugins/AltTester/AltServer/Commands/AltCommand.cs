using System;
using System.Globalization;
using Altom.AltDriver;
using Altom.AltDriver.Commands;
using Newtonsoft.Json;

namespace Altom.AltTester.Commands
{
    public abstract class AltCommand<TParam, TResult> where TParam : CommandParams
    {
        private const int MAX_DEPTH_REPONSE_DATA_SERIALIZATION = 2;
        public TParam CommandParams { get; private set; }

        protected AltCommand(TParam commandParams)
        {
            CommandParams = commandParams;
        }

        public string ExecuteAndSerialize<T>(Func<T> action)
        {
            var result = ExecuteHandleErrors(action);
            return JsonConvert.SerializeObject(result, new JsonSerializerSettings
            {
                Culture = CultureInfo.InvariantCulture
            });
        }

        public string ExecuteAndSerialize()
        {
            return ExecuteAndSerialize(Execute);
        }

        protected CommandResponse ExecuteHandleErrors<T>(Func<T> action)
        {
            T response = default(T);
            Exception exception = null;
            CommandError error = null;
            String errorType = null;

            try
            {
                response = action();
            }
            catch (System.NullReferenceException e)
            {
                exception = e;
                errorType = AltErrors.errorNullReference;
            }
            catch (FailedToParseArgumentsException e)
            {
                exception = e;
                errorType = AltErrors.errorFailedToParseArguments;
            }
            catch (MethodWithGivenParametersNotFoundException e)
            {
                exception = e;
                errorType = AltErrors.errorMethodWithGivenParametersNotFound;
            }
            catch (InvalidParameterTypeException e)
            {
                exception = e;
                errorType = AltErrors.errorInvalidParameterType;
            }
            catch (JsonException e)
            {
                exception = e;
                errorType = AltErrors.errorCouldNotParseJsonString;
            }
            catch (ComponentNotFoundException e)
            {
                exception = e;
                errorType = AltErrors.errorComponentNotFound;
            }
            catch (MethodNotFoundException e)
            {
                exception = e;
                errorType = AltErrors.errorMethodNotFound;
            }
            catch (PropertyNotFoundException e)
            {
                exception = e;
                errorType = AltErrors.errorPropertyNotFound;
            }
            catch (AssemblyNotFoundException e)
            {
                exception = e;
                errorType = AltErrors.errorAssemblyNotFound;
            }
            catch (CouldNotPerformOperationException e)
            {
                exception = e;
                errorType = AltErrors.errorCouldNotPerformOperation;
            }
            catch (InvalidPathException e)
            {
                exception = e;
                errorType = AltErrors.errorInvalidPath;
            }
            catch (NotFoundException e)
            {
                exception = e;
                errorType = AltErrors.errorNotFound;
            }
            catch (SceneNotFoundException e)
            {
                exception = e;
                errorType = AltErrors.errorSceneNotFound;
            }
            catch (CameraNotFoundException e)
            {
                exception = e;
                errorType = AltErrors.errorCameraNotFound;
            }
            catch (InvalidCommandException e)
            {
                exception = e.InnerException;
                errorType = AltErrors.errorInvalidCommand;
            }
            catch (AltInnerException e)
            {
                exception = e.InnerException;
                errorType = AltErrors.errorUnknownError;
            }
            catch (Exception e)
            {
                exception = e;
                errorType = AltErrors.errorUnknownError;
            }

            if (exception != null)
            {
                error = new CommandError();
                error.type = errorType;
                error.message = exception.Message;
                error.trace = exception.StackTrace;
            }

            var cmdResponse = new CommandResponse();
            cmdResponse.commandName = CommandParams.commandName;
            cmdResponse.messageId = CommandParams.messageId;

            if (response != null)
            {
                int maxDepth = MAX_DEPTH_REPONSE_DATA_SERIALIZATION;

                if (CommandParams is AltGetObjectComponentPropertyParams)
                {
                    maxDepth = (CommandParams as AltGetObjectComponentPropertyParams).maxDepth;
                }
                try
                {
                    using (var strWriter = new System.IO.StringWriter())
                    {
                        using (var jsonWriter = new CustomJsonTextWriter(strWriter))
                        {
                            Func<bool> include = () => jsonWriter.CurrentDepth <= maxDepth;
                            var resolver = new AltContractResolver(include);
                            var serializer = new Newtonsoft.Json.JsonSerializer { ContractResolver = resolver, ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore };
                            serializer.Serialize(jsonWriter, response);
                        }
                        cmdResponse.data = strWriter.ToString();
                    }
                }
                catch (Exception e)
                {
                    error = new CommandError();
                    error.type = AltErrors.errorUnknownError;
                    error.message = e.Message;
                    error.trace = e.StackTrace;
                }

            }

            cmdResponse.error = error;
            cmdResponse.isNotification = false;

            return cmdResponse;
        }

        public abstract TResult Execute();
    }
}
