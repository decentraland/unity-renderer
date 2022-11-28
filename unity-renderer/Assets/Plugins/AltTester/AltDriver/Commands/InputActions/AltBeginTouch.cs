namespace Altom.AltDriver.Commands
{
    public class AltBeginTouch : AltBaseCommand
    {
        AltBeginTouchParams cmdParams;

        public AltBeginTouch(IDriverCommunication commHandler, AltVector2 coordinates) : base(commHandler)
        {
            this.cmdParams = new AltBeginTouchParams(coordinates);
        }
        public int Execute()
        {
            CommHandler.Send(cmdParams);
            return CommHandler.Recvall<int>(cmdParams);  //finger id
            //TODO: add handling for "Finished"
        }
    }
}