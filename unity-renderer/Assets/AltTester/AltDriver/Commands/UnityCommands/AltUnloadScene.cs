namespace Altom.AltDriver.Commands
{
    public class AltUnloadScene : AltBaseCommand
    {
        AltUnloadSceneParams cmdParams;
        public AltUnloadScene(IDriverCommunication commHandler, string sceneName) : base(commHandler)
        {
            cmdParams = new AltUnloadSceneParams(sceneName);
        }
        public void Execute()
        {
            CommHandler.Send(cmdParams);

            var data = CommHandler.Recvall<string>(cmdParams);
            ValidateResponse("Ok", data);

            data = CommHandler.Recvall<string>(cmdParams);
            ValidateResponse("Scene Unloaded", data);
        }
    }
}