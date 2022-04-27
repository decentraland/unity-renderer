using System;

namespace DCL
{
    public static class ProfilingEvents
    {
        //NOTE(Brian): For performance reasons, these events may need to be removed for production.
        public static Action<string> OnMessageWillQueue;
        public static Action<string> OnMessageWillDequeue;

        public static Action<string> OnMessageProcessStart;
        public static Action<string> OnMessageProcessEnds;

        public static Action<string> OnMessageDecodeStart;
        public static Action<string> OnMessageDecodeEnds;
    }
}