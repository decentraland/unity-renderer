using System.Collections.Generic;
using UnityEngine;

namespace MainScripts.DCL.Analytics.PerformanceAnalytics
{
    public class SimpleTracker
    {
        private int count;
        
        public int Get() => count;

        public void Track()
        {
            count++;
        }
        
        public void Reset()
        {
            count = 0;
        }
    }
    public class LoadingTracker
    {
        private int loading;
        private int failed;
        private int cancelled;
        private int loaded;
        
        public void TrackLoading()
        {
            loading++;
        }
        
        public void TrackFailed()
        {
            loading--;
            failed++;
        }

        public void TrackCancelled()
        {
            loading--;
            cancelled++;
        }
        
        public void TrackLoaded()
        {
            loading--;
            loaded++;
        }

        public void Reset()
        {
            failed = 0;
            cancelled = 0;
            loaded = 0;
            loading = 0;
        }
        public (int loading, int failed, int cancelled, int loaded) GetData() { return (loading, failed, cancelled, loaded); }
    }
    
    public static class PerformanceAnalytics
    {
        public static readonly LoadingTracker GLTFTracker = new LoadingTracker();
        public static readonly LoadingTracker ABTracker = new LoadingTracker();

        public static readonly SimpleTracker GLTFTextureTracker = new SimpleTracker();
        public static readonly SimpleTracker ABTextureTracker = new SimpleTracker();
        public static readonly SimpleTracker PromiseTextureTracker = new SimpleTracker();

        public static readonly SimpleTracker MessagesEnqueuedTracker = new SimpleTracker();
        public static readonly SimpleTracker MessagesProcessedTracker = new SimpleTracker();

        public static void ResetAll()
        {
            GLTFTracker.Reset();
            ABTracker.Reset();
            GLTFTextureTracker.Reset();
            ABTextureTracker.Reset();
            PromiseTextureTracker.Reset();
            MessagesEnqueuedTracker.Reset();
            MessagesProcessedTracker.Reset();
        }
    }
}