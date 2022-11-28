namespace Altom.AltDriver.Commands
{
    public class AltGetPNGScreenshot : AltBaseCommand
    {
        string path;
        AltGetPNGScreenshotParams cmdParams;
        public AltGetPNGScreenshot(IDriverCommunication commHandler, string path) : base(commHandler)
        {
            this.path = path;
            this.cmdParams = new AltGetPNGScreenshotParams();
        }
        public void Execute()
        {
            CommHandler.Send(cmdParams);
            var message = CommHandler.Recvall<string>(cmdParams);
            ValidateResponse("Ok", message);
            string screenshotData = CommHandler.Recvall<string>(cmdParams);
            System.IO.File.WriteAllBytes(path, System.Convert.FromBase64String(screenshotData));
        }
    }
}