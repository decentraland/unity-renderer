
using System;
using Altom.AltDriver.Logging;

namespace Altom.AltDriver
{
    public class AltErrors
    {
        public const string errorNotFound = "notFound";
        public const string errorSceneNotFound = "sceneNotFound";
        public const string errorPropertyNotFound = "propertyNotFound";
        public const string errorMethodNotFound = "methodNotFound";
        public const string errorComponentNotFound = "componentNotFound";
        public const string errorAssemblyNotFound = "assemblyNotFound";
        public const string errorCouldNotPerformOperation = "couldNotPerformOperation";
        public const string errorCouldNotParseJsonString = "couldNotParseJsonString";
        public const string errorMethodWithGivenParametersNotFound = "methodWithGivenParametersNotFound";
        public const string errorInvalidParameterType = "invalidParameterType";
        public const string errorFailedToParseArguments = "failedToParseMethodArguments";
        public const string errorObjectWasNotFound = "objectNotFound";
        public const string errorPropertyNotSet = "propertyCannotBeSet";
        public const string errorNullReference = "nullReferenceException";
        public const string errorUnknownError = "unknownError";
        public const string errorFormatException = "formatException";
        public const string errorCameraNotFound = "cameraNotFound";
        public const string errorIndexOutOfRange = "indexOutOfRange";
        public const string errorInvalidCommand = "invalidCommand";
        public const string errorInvalidPath = "invalidPath";
        public const string errorInputModule = "ALTTESTERNotAddedAsDefineVariable";
    }

    public class AltException : Exception
    {
        public AltException()
        {

        }

        public AltException(string message) : base(message)
        {

        }
        public AltException(string message, Exception inner) : base(message, inner)
        {

        }
    }

    public class NotFoundException : AltException
    {
        public NotFoundException()
        {
        }

        public NotFoundException(string message) : base(message)
        {
        }
    }

    public class CameraNotFoundException : AltException
    {
        public CameraNotFoundException()
        {
        }

        public CameraNotFoundException(string message) : base(message)
        {
        }
    }

    public class PropertyNotFoundException : AltException
    {
        public PropertyNotFoundException()
        {
        }

        public PropertyNotFoundException(string message) : base(message)
        {
        }
    }

    public class MethodNotFoundException : AltException
    {
        public MethodNotFoundException()
        {
        }

        public MethodNotFoundException(string message) : base(message)
        {
        }
    }

    public class ComponentNotFoundException : AltException
    {
        public ComponentNotFoundException()
        {
        }

        public ComponentNotFoundException(string message) : base(message)
        {
        }
    }

    public class AssemblyNotFoundException : AltException
    {
        public AssemblyNotFoundException()
        {
        }

        public AssemblyNotFoundException(string message) : base(message)
        {
        }
    }

    public class CouldNotPerformOperationException : AltException
    {
        public CouldNotPerformOperationException()
        {
        }

        public CouldNotPerformOperationException(string message) : base(message)
        {
        }
    }

    public class InvalidParameterTypeException : AltException
    {
        public InvalidParameterTypeException()
        {
        }

        public InvalidParameterTypeException(string message) : base(message)
        {
        }
    }

    public class MethodWithGivenParametersNotFoundException : AltException
    {
        public MethodWithGivenParametersNotFoundException()
        {
        }

        public MethodWithGivenParametersNotFoundException(string message) : base(message)
        {
        }
    }

    public class FailedToParseArgumentsException : AltException
    {
        public FailedToParseArgumentsException()
        {
        }

        public FailedToParseArgumentsException(string message) : base(message)
        {
        }
    }

    public class ObjectWasNotFoundException : AltException
    {
        public ObjectWasNotFoundException()
        {
        }

        public ObjectWasNotFoundException(string message) : base(message)
        {
        }
    }

    public class NullReferenceException : AltException
    {
        public NullReferenceException()
        {
        }

        public NullReferenceException(string message) : base(message)
        {
        }
    }

    public class UnknownErrorException : AltException
    {
        public UnknownErrorException()
        {
        }

        public UnknownErrorException(string message) : base(message)
        {
        }
    }

    public class FormatException : AltException
    {
        public FormatException()
        {
        }

        public FormatException(string message) : base(message)
        {
        }
    }

    public class WaitTimeOutException : AltException
    {
        public WaitTimeOutException()
        {
        }

        public WaitTimeOutException(string message) : base(message)
        {
        }
    }

    public class CommandResponseTimeoutException : AltException
    {
        public CommandResponseTimeoutException()
        {
        }

        public CommandResponseTimeoutException(string message) : base(message)
        {
        }
    }

    /// <summary>
    /// Raised when the client connection timesout.
    /// </summary>
    public class ConnectionTimeoutException : AltException
    {
        public ConnectionTimeoutException()
        {

        }
        public ConnectionTimeoutException(string message) : base(message)
        {

        }
    }

    /// <summary>
    /// Raised when the client can not connect to the server
    /// </summary>
    public class ConnectionException : AltException
    {
        public ConnectionException()
        {

        }
        public ConnectionException(string message) : base(message)
        {

        }
    }

    public class CommandNotFoundException : AltException
    {
        public CommandNotFoundException()
        {
        }

        public CommandNotFoundException(string message) : base(message)
        {
        }
    }

    public class InvalidCommandException : AltException
    {
        public InvalidCommandException(Exception innerException) : base(AltErrors.errorInvalidCommand, innerException)
        {
        }

        public InvalidCommandException(string message) : base(message)
        {
        }
    }

    public class AltRecvallException : AltException
    {
        public AltRecvallException()
        {

        }

        public AltRecvallException(string message) : base(message)
        {

        }
    }

    public class AltRecvallMessageIdException : AltRecvallException
    {
        public AltRecvallMessageIdException()
        {

        }

        public AltRecvallMessageIdException(string message) : base(message)
        {

        }
    }

    public class AltRecvallMessageFormatException : AltRecvallException
    {
        public AltRecvallMessageFormatException()
        {

        }

        public AltRecvallMessageFormatException(string message) : base(message)
        {
        }
    }

    public class PortForwardingException : AltException
    {
        public PortForwardingException()
        {
        }

        public PortForwardingException(string message) : base(message)
        {
        }

        public PortForwardingException(string message, Exception inner) : base(message, inner)
        {
        }
    }


    public class AltInvalidServerResponse : AltException
    {
        public AltInvalidServerResponse(string expected, string received) : base(string.Format("Expected to get response '{0}'; Got  '{1}'", expected, received))
        {
        }
    }

    public class InvalidPathException : AltException
    {
        public InvalidPathException()
        {
        }

        public InvalidPathException(string message) : base(message)
        {
        }
    }

    public class AltInputModuleException : AltException
    {

        public AltInputModuleException()
        {
        }

        public AltInputModuleException(string message) : base(message)
        {
        }
    }

    public class AltInnerException : AltException
    {

        public AltInnerException(Exception inner) : base(AltErrors.errorUnknownError, inner)
        {
        }
    }

    public class AltCameraNotFoundException : AltException
    {
        public AltCameraNotFoundException()
        {
        }

        public AltCameraNotFoundException(string message) : base(message)
        {
        }
    }

    public class AltPathNotFoundException : AltException
    {
        public AltPathNotFoundException()
        {
        }

        public AltPathNotFoundException(string message) : base(message)
        {
        }
    }

    public class SceneNotFoundException : AltException
    {
        public SceneNotFoundException()
        {
        }

        public SceneNotFoundException(string message) : base(message)
        {
        }
    }

    public class ResponseFormatException : AltException
    {
        public ResponseFormatException() { }
        public ResponseFormatException(Type t, string data) : base("Could not deserialize response data: `" + data + "` into " + t.FullName)
        {

        }
    }
}
