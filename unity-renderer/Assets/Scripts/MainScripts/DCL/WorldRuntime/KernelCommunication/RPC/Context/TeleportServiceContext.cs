using System.Collections.Generic;

namespace RPC.Context
{
    public class TeleportServiceContext
    {
        public readonly Queue<Teleport.Types.FromRenderer> queueMessages = new Queue<Teleport.Types.FromRenderer>();

        public void JumpIn(int x, int y, string serverName)
        {
            queueMessages.Enqueue(new Teleport.Types.FromRenderer()
            {
                JumpIn = { ParcelX = x, ParcelY = y, Realm = serverName }
            });
        }
        
        public void TeleportTo(int x, int y)
        {
            queueMessages.Enqueue(new Teleport.Types.FromRenderer()
            {
                TeleportTo = { X = x, Y = y }
            });
        }
        
        public void TeleportToCrowd()
        {
            queueMessages.Enqueue(new Teleport.Types.FromRenderer()
            {
                TeleportToCrowd = {}
            });
        }
                
        public void TeleportToMagic()
        {
            queueMessages.Enqueue(new Teleport.Types.FromRenderer()
            {
                TeleportToMagic = {}
            });
        }
    }
}
