using Newtonsoft.Json;

namespace Altom.AltDriver.Commands
{
    public class AltLoadScene : AltBaseCommand
    {
        AltLoadSceneParams cmdParams;
        public AltLoadScene(IDriverCommunication commHandler, string sceneName, bool loadSingle) : base(commHandler)
        {
            cmdParams = new AltLoadSceneParams(sceneName, loadSingle);
        }
        public void Execute()
        {
            CommHandler.Send(cmdParams);

            var data = CommHandler.Recvall<string>(cmdParams);
            ValidateResponse("Ok", data);

            data = CommHandler.Recvall<string>(cmdParams);
            ValidateResponse("Scene Loaded", data);
        }
    }
}