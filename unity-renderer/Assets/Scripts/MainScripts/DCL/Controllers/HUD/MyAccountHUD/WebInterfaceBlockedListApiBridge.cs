using DCL.Interface;

namespace DCL.MyAccount
{
    public class WebInterfaceBlockedListApiBridge : IBlockedListApiBridge
    {
        public WebInterfaceBlockedListApiBridge() { }

        public void SendUnblockPlayer(string playerId)
        {
            WebInterface.SendUnblockPlayer(playerId);
        }
    }
}
