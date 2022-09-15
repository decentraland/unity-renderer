using System.Collections.Generic;

namespace RPC.Context
{
    public static class TeleportServiceContextEnqueue
    {
        public static void JumpIn(this TeleportServiceContext self, int x, int y, string serverName)
        {
            self.queueMessages.Enqueue(new Teleport.Types.FromRenderer()
            {
                JumpIn = { ParcelX = x, ParcelY = y, Realm = serverName }
            });
        }
        
        public static void TeleportTo(this TeleportServiceContext self, int x, int y)
        {
            self.queueMessages.Enqueue(new Teleport.Types.FromRenderer()
            {
                TeleportTo = { X = x, Y = y }
            });
        }
        
        public static void TeleportToCrowd(this TeleportServiceContext self)
        {
            self.queueMessages.Enqueue(new Teleport.Types.FromRenderer()
            {
                TeleportToCrowd = {}
            });
        }
                
        public static void TeleportToMagic(this TeleportServiceContext self)
        {
            self.queueMessages.Enqueue(new Teleport.Types.FromRenderer()
            {
                TeleportToMagic = {}
            });
        }
    }
}