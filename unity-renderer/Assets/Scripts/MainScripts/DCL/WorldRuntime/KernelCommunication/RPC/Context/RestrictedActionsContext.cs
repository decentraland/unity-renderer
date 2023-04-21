using System;

namespace RPC.Context
{
    public class RestrictedActionsContext
    {
        public Func<string, int, bool> OpenExternalUrlPrompt;
        public Action<string, string> OpenNftPrompt;
    }
}
